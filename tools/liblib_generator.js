/**
 * LiblibAI 图片生成脚本
 * 使用 LiblibAI API 生成游戏美术资源
 */

const crypto = require('crypto');
const https = require('https');
const http = require('http');
const fs = require('fs');
const path = require('path');

// API 配置
const ACCESS_KEY = "r0LTLSLDq4e3a-PFdmrBpA";
const SECRET_KEY = "4lH_K_Ya66ENhg-h_C64YnpnZdUoU6iJ";
const BASE_URL = "openapi.liblibai.cloud";
const TEXT2IMG_URI = "/api/generate/webui/text2img";
const STATUS_URI = "/api/generate/webui/status";

// 输出目录
const OUTPUT_DIR = "E:\\AI_Project\\MS\\MoShou\\Assets\\Resources\\Sprites\\Generated";

// 任务文件
const TASK_FILE = "E:\\AI_Project\\MS\\.task\\art_generation_task.json";

/**
 * 生成 HMAC-SHA1 签名
 */
function generateSignature(uri, timestamp, nonce) {
    const stringToSign = `${uri}&${timestamp}&${nonce}`;
    const hmac = crypto.createHmac('sha1', SECRET_KEY);
    hmac.update(stringToSign);
    const signature = hmac.digest('base64')
        .replace(/\+/g, '-')
        .replace(/\//g, '_')
        .replace(/=+$/, '');
    return signature;
}

/**
 * 生成随机字符串
 */
function generateNonce(length = 16) {
    const chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
    let result = '';
    for (let i = 0; i < length; i++) {
        result += chars.charAt(Math.floor(Math.random() * chars.length));
    }
    return result;
}

/**
 * 发送 API 请求
 */
function makeApiRequest(uri, payload) {
    return new Promise((resolve, reject) => {
        const timestamp = Date.now().toString();
        const nonce = generateNonce();
        const signature = generateSignature(uri, timestamp, nonce);

        const queryParams = new URLSearchParams({
            AccessKey: ACCESS_KEY,
            Signature: signature,
            Timestamp: timestamp,
            SignatureNonce: nonce
        });

        const body = JSON.stringify(payload);

        const options = {
            hostname: BASE_URL,
            port: 443,
            path: `${uri}?${queryParams.toString()}`,
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Content-Length': Buffer.byteLength(body)
            }
        };

        console.log(`[DEBUG] Request URL: https://${BASE_URL}${uri}?${queryParams.toString()}`);

        const req = https.request(options, (res) => {
            let data = '';
            res.on('data', chunk => data += chunk);
            res.on('end', () => {
                try {
                    resolve(JSON.parse(data));
                } catch (e) {
                    console.error('[ERROR] Failed to parse response:', data);
                    reject(e);
                }
            });
        });

        req.on('error', reject);
        req.write(body);
        req.end();
    });
}

/**
 * 检查生成任务状态
 */
function checkStatus(generateUuid) {
    return makeApiRequest(STATUS_URI, { generateUuid });
}

/**
 * 下载图片
 */
function downloadImage(url, filename) {
    return new Promise((resolve, reject) => {
        const filepath = path.join(OUTPUT_DIR, filename);
        const file = fs.createWriteStream(filepath);

        const protocol = url.startsWith('https') ? https : http;
        protocol.get(url, (response) => {
            // 处理重定向
            if (response.statusCode === 301 || response.statusCode === 302) {
                downloadImage(response.headers.location, filename)
                    .then(resolve)
                    .catch(reject);
                return;
            }

            response.pipe(file);
            file.on('finish', () => {
                file.close();
                console.log(`[SUCCESS] Downloaded: ${filename}`);
                resolve(true);
            });
        }).on('error', (err) => {
            fs.unlink(filepath, () => {}); // 删除失败的文件
            console.error(`[ERROR] Download failed: ${err.message}`);
            reject(err);
        });
    });
}

/**
 * 等待指定毫秒
 */
function sleep(ms) {
    return new Promise(resolve => setTimeout(resolve, ms));
}

/**
 * 生成单张图片
 */
async function generateImage(prompt, negativePrompt, width, height, filename) {
    console.log(`\n[INFO] Generating: ${filename}`);
    console.log(`  Prompt: ${prompt.substring(0, 50)}...`);

    // 构建请求payload
    const payload = {
        templateUuid: "5d7e67009b344550bc1aa6ccbfa1d7f4",  // 通用文生图模板
        generateParams: {
            prompt: prompt,
            negativePrompt: negativePrompt,
            width: width,
            height: height,
            steps: 20,
            cfgScale: 7,
            seed: -1,
            samplerName: "DPM++ 2M Karras"
        }
    };

    try {
        // 发送生成请求
        const result = await makeApiRequest(TEXT2IMG_URI, payload);
        console.log('[DEBUG] Generate response:', JSON.stringify(result).substring(0, 200));

        if (result.code !== 0) {
            console.error(`[ERROR] Generation failed: ${JSON.stringify(result)}`);
            return false;
        }

        const generateUuid = result.data?.generateUuid;
        if (!generateUuid) {
            console.error('[ERROR] No generateUuid in response');
            return false;
        }

        console.log(`  Task UUID: ${generateUuid}`);

        // 轮询状态
        const maxAttempts = 60;
        for (let attempt = 0; attempt < maxAttempts; attempt++) {
            await sleep(5000); // 每5秒检查一次

            const statusResult = await checkStatus(generateUuid);
            console.log(`[DEBUG] Full status response: ${JSON.stringify(statusResult).substring(0, 500)}`);

            const status = statusResult.data?.generateStatus || statusResult.data?.status;
            const percentCompleted = statusResult.data?.percentCompleted || statusResult.data?.progress || 0;

            console.log(`  Status check ${attempt + 1}/${maxAttempts}: ${status} (${percentCompleted}%)`);

            // generateStatus: 5 = SUCCESS, 4 = PROCESSING, 2 = FAILED
            if (status === 5 || status === 'SUCCESS') {
                const images = statusResult.data?.images || [];
                if (images.length > 0) {
                    const imageUrl = images[0].imageUrl;
                    if (imageUrl) {
                        return await downloadImage(imageUrl, filename);
                    }
                }
                console.error('[ERROR] No image URL in response');
                return false;
            } else if (status === 2 || status === 'FAILED') {
                console.error('[ERROR] Generation failed');
                return false;
            }
        }

        console.error('[ERROR] Timeout waiting for generation');
        return false;

    } catch (error) {
        console.error(`[ERROR] Exception: ${error.message}`);
        return false;
    }
}

/**
 * 主函数
 */
async function main() {
    console.log('='.repeat(60));
    console.log('LiblibAI Art Generator');
    console.log('='.repeat(60));

    // 确保输出目录存在
    if (!fs.existsSync(OUTPUT_DIR)) {
        fs.mkdirSync(OUTPUT_DIR, { recursive: true });
    }

    // 读取任务文件
    let taskData;
    try {
        const taskContent = fs.readFileSync(TASK_FILE, 'utf-8');
        taskData = JSON.parse(taskContent);
    } catch (error) {
        console.error(`[ERROR] Failed to read task file: ${error.message}`);
        process.exit(1);
    }

    const requests = taskData.generation_requests || [];
    console.log(`[INFO] Found ${requests.length} generation requests`);
    console.log(`[INFO] Output directory: ${OUTPUT_DIR}`);

    let successCount = 0;
    let failCount = 0;

    // 只处理第一个pending的任务作为测试
    for (const req of requests) {
        if (req.status === 'completed') {
            console.log(`\n[SKIP] Already completed: ${req.filename}`);
            continue;
        }

        const success = await generateImage(
            req.prompt,
            req.negative_prompt || '',
            req.width,
            req.height,
            req.filename
        );

        if (success) {
            req.status = 'completed';
            successCount++;
        } else {
            req.status = 'failed';
            failCount++;
        }

        // 更新任务文件
        fs.writeFileSync(TASK_FILE, JSON.stringify(taskData, null, 2), 'utf-8');

        // 为避免API限流，等待一下
        console.log('\n[INFO] Waiting 3 seconds before next request...');
        await sleep(3000);
    }

    console.log(`\n${'='.repeat(60)}`);
    console.log(`[DONE] Success: ${successCount}, Failed: ${failCount}`);
}

main().catch(console.error);
