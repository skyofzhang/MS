using UnityEngine;
using UnityEditor;
using UnityEditor.Compilation;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MoShou.Editor
{
    /// <summary>
    /// 编译错误上报器 - 自动将编译错误发送到 n8n 触发修复流程
    /// </summary>
    [InitializeOnLoad]
    public static class BuildErrorReporter
    {
        // n8n Webhook 地址
        private const string N8N_WEBHOOK_URL = "http://43.161.249.54:5678/webhook/build-error";

        // 上次上报时间（防止重复上报）
        private static DateTime lastReportTime = DateTime.MinValue;
        private static readonly TimeSpan ReportCooldown = TimeSpan.FromSeconds(10);

        // 错误缓存
        private static List<CompilerMessage> cachedErrors = new List<CompilerMessage>();

        static BuildErrorReporter()
        {
            // 订阅编译完成事件
            CompilationPipeline.compilationFinished += OnCompilationFinished;

            // 订阅编译消息事件
            CompilationPipeline.assemblyCompilationFinished += OnAssemblyCompilationFinished;

            Debug.Log("[BuildErrorReporter] 编译错误监听已启动");
        }

        /// <summary>
        /// 单个程序集编译完成
        /// </summary>
        private static void OnAssemblyCompilationFinished(string assemblyPath, CompilerMessage[] messages)
        {
            foreach (var message in messages)
            {
                if (message.type == CompilerMessageType.Error)
                {
                    cachedErrors.Add(message);
                }
            }
        }

        /// <summary>
        /// 整体编译完成
        /// </summary>
        private static void OnCompilationFinished(object context)
        {
            if (cachedErrors.Count > 0)
            {
                // 有编译错误
                Debug.LogWarning($"[BuildErrorReporter] 检测到 {cachedErrors.Count} 个编译错误");

                // 检查冷却时间
                if (DateTime.Now - lastReportTime < ReportCooldown)
                {
                    Debug.Log("[BuildErrorReporter] 冷却中，跳过上报");
                    cachedErrors.Clear();
                    return;
                }

                // 上报错误
                ReportErrorsToN8N(cachedErrors);
                lastReportTime = DateTime.Now;
            }
            else
            {
                Debug.Log("[BuildErrorReporter] 编译成功，无错误");
            }

            cachedErrors.Clear();
        }

        /// <summary>
        /// 上报错误到 n8n
        /// </summary>
        private static async void ReportErrorsToN8N(List<CompilerMessage> errors)
        {
            try
            {
                // 构建错误报告
                var errorReport = new ErrorReport
                {
                    project = "MoShou",
                    timestamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    error_count = errors.Count,
                    errors = new List<ErrorDetail>()
                };

                foreach (var error in errors)
                {
                    errorReport.errors.Add(new ErrorDetail
                    {
                        file = error.file,
                        line = error.line,
                        column = error.column,
                        message = error.message,
                        code = ExtractErrorCode(error.message)
                    });
                }

                // 转换为 JSON
                string json = JsonUtility.ToJson(errorReport, true);

                // 同时保存到本地文件（供 Claude 读取）
                string errorFilePath = Path.Combine(Application.dataPath, "..", ".task", "build_errors.json");
                Directory.CreateDirectory(Path.GetDirectoryName(errorFilePath));
                File.WriteAllText(errorFilePath, json);

                Debug.Log($"[BuildErrorReporter] 错误已保存到: {errorFilePath}");

                // 发送到 n8n
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(10);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync(N8N_WEBHOOK_URL, content);

                    if (response.IsSuccessStatusCode)
                    {
                        Debug.Log("[BuildErrorReporter] 错误已上报到 n8n");
                    }
                    else
                    {
                        Debug.LogWarning($"[BuildErrorReporter] n8n 上报失败: {response.StatusCode}");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[BuildErrorReporter] 上报异常: {e.Message}");
                // 即使 n8n 不可用，错误文件已保存到本地
            }
        }

        /// <summary>
        /// 提取错误代码 (如 CS0246)
        /// </summary>
        private static string ExtractErrorCode(string message)
        {
            // 尝试匹配 CS#### 格式的错误代码
            var match = System.Text.RegularExpressions.Regex.Match(message, @"CS\d{4}");
            return match.Success ? match.Value : "UNKNOWN";
        }

        /// <summary>
        /// 手动触发错误上报（用于测试）
        /// </summary>
        [MenuItem("MoShou/Debug/Report Build Errors")]
        public static void ManualReportErrors()
        {
            // 强制刷新编译
            AssetDatabase.Refresh();
            Debug.Log("[BuildErrorReporter] 手动触发编译检查");
        }

        /// <summary>
        /// 测试 n8n 连接
        /// </summary>
        [MenuItem("MoShou/Debug/Test n8n Connection")]
        public static async void TestN8NConnection()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(5);

                    var testPayload = new
                    {
                        test = true,
                        message = "Connection test from Unity",
                        timestamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ")
                    };

                    string json = JsonUtility.ToJson(testPayload);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync(N8N_WEBHOOK_URL, content);

                    Debug.Log($"[BuildErrorReporter] n8n 连接测试: {response.StatusCode}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[BuildErrorReporter] n8n 连接失败: {e.Message}");
            }
        }
    }

    /// <summary>
    /// 错误报告数据结构
    /// </summary>
    [Serializable]
    public class ErrorReport
    {
        public string project;
        public string timestamp;
        public int error_count;
        public List<ErrorDetail> errors;
    }

    /// <summary>
    /// 错误详情
    /// </summary>
    [Serializable]
    public class ErrorDetail
    {
        public string file;
        public int line;
        public int column;
        public string message;
        public string code;
    }
}
