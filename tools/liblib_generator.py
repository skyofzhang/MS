#!/usr/bin/env python3
"""
LiblibAI 图片生成脚本
使用 LiblibAI API 生成游戏美术资源
"""

import hmac
import hashlib
import base64
import time
import random
import string
import json
import requests
import os
from urllib.parse import urlencode

# API 配置
ACCESS_KEY = "r0LTLSLDq4e3a-PFdmrBpA"
SECRET_KEY = "4lH_K_Ya66ENhg-h_C64YnpnZdUoU6iJ"
BASE_URL = "https://openapi.liblibai.cloud"
TEXT2IMG_URI = "/api/generate/webui/text2img"
STATUS_URI = "/api/generate/webui/status"

# 输出目录
OUTPUT_DIR = r"E:\AI_Project\MS\MoShou\Assets\Resources\Sprites\Generated"

def generate_signature(uri, timestamp, nonce):
    """生成 HMAC-SHA1 签名"""
    # 拼接待签名字符串
    string_to_sign = f"{uri}&{timestamp}&{nonce}"

    # HMAC-SHA1 加密
    signature = hmac.new(
        SECRET_KEY.encode('utf-8'),
        string_to_sign.encode('utf-8'),
        hashlib.sha1
    ).digest()

    # Base64 URL-safe 编码并移除尾部等号
    signature_b64 = base64.urlsafe_b64encode(signature).decode('utf-8').rstrip('=')

    return signature_b64

def generate_nonce(length=16):
    """生成随机字符串"""
    return ''.join(random.choices(string.ascii_letters + string.digits, k=length))

def make_api_request(uri, payload):
    """发送 API 请求"""
    timestamp = str(int(time.time() * 1000))
    nonce = generate_nonce()
    signature = generate_signature(uri, timestamp, nonce)

    # 构建查询参数
    query_params = {
        "AccessKey": ACCESS_KEY,
        "Signature": signature,
        "Timestamp": timestamp,
        "SignatureNonce": nonce
    }

    url = f"{BASE_URL}{uri}?{urlencode(query_params)}"

    headers = {
        "Content-Type": "application/json"
    }

    response = requests.post(url, json=payload, headers=headers)
    return response.json()

def check_status(generate_uuid):
    """检查生成任务状态"""
    timestamp = str(int(time.time() * 1000))
    nonce = generate_nonce()
    signature = generate_signature(STATUS_URI, timestamp, nonce)

    query_params = {
        "AccessKey": ACCESS_KEY,
        "Signature": signature,
        "Timestamp": timestamp,
        "SignatureNonce": nonce
    }

    url = f"{BASE_URL}{STATUS_URI}?{urlencode(query_params)}"

    payload = {
        "generateUuid": generate_uuid
    }

    response = requests.post(url, json=payload, headers={"Content-Type": "application/json"})
    return response.json()

def download_image(url, filename):
    """下载图片到本地"""
    response = requests.get(url)
    if response.status_code == 200:
        filepath = os.path.join(OUTPUT_DIR, filename)
        with open(filepath, 'wb') as f:
            f.write(response.content)
        print(f"[SUCCESS] Downloaded: {filename}")
        return True
    else:
        print(f"[ERROR] Failed to download: {filename}")
        return False

def generate_image(prompt, negative_prompt, width, height, filename):
    """生成单张图片"""
    print(f"\n[INFO] Generating: {filename}")
    print(f"  Prompt: {prompt[:50]}...")

    # 构建请求payload - 使用通用模板
    payload = {
        "templateUuid": "5d7e67009b344550bc1aa6ccbfa1d7f4",  # 通用文生图模板
        "generateParams": {
            "prompt": prompt,
            "negativePrompt": negative_prompt,
            "width": width,
            "height": height,
            "steps": 20,
            "cfgScale": 7,
            "seed": -1,  # 随机种子
            "samplerName": "DPM++ 2M Karras"
        }
    }

    # 发送生成请求
    result = make_api_request(TEXT2IMG_URI, payload)

    if result.get("code") != 0:
        print(f"[ERROR] Generation failed: {result}")
        return False

    generate_uuid = result.get("data", {}).get("generateUuid")
    if not generate_uuid:
        print(f"[ERROR] No generateUuid in response")
        return False

    print(f"  Task UUID: {generate_uuid}")

    # 轮询状态
    max_attempts = 60
    for attempt in range(max_attempts):
        time.sleep(5)  # 每5秒检查一次

        status_result = check_status(generate_uuid)
        status = status_result.get("data", {}).get("status")

        print(f"  Status check {attempt + 1}/{max_attempts}: {status}")

        if status == "SUCCESS":
            # 获取图片URL
            images = status_result.get("data", {}).get("images", [])
            if images:
                image_url = images[0].get("imageUrl")
                if image_url:
                    return download_image(image_url, filename)
            print(f"[ERROR] No image URL in response")
            return False
        elif status == "FAILED":
            print(f"[ERROR] Generation failed")
            return False

    print(f"[ERROR] Timeout waiting for generation")
    return False

def main():
    """主函数 - 生成所有待处理的资源"""

    # 确保输出目录存在
    os.makedirs(OUTPUT_DIR, exist_ok=True)

    # 读取任务文件
    task_file = r"E:\AI_Project\MS\.task\art_generation_task.json"
    with open(task_file, 'r', encoding='utf-8') as f:
        task_data = json.load(f)

    requests_list = task_data.get("generation_requests", [])

    print(f"[INFO] Found {len(requests_list)} generation requests")
    print(f"[INFO] Output directory: {OUTPUT_DIR}")

    success_count = 0
    fail_count = 0

    for req in requests_list:
        if req.get("status") == "completed":
            print(f"\n[SKIP] Already completed: {req['filename']}")
            continue

        success = generate_image(
            prompt=req["prompt"],
            negative_prompt=req.get("negative_prompt", ""),
            width=req["width"],
            height=req["height"],
            filename=req["filename"]
        )

        if success:
            req["status"] = "completed"
            success_count += 1
        else:
            req["status"] = "failed"
            fail_count += 1

        # 更新任务文件
        with open(task_file, 'w', encoding='utf-8') as f:
            json.dump(task_data, f, indent=2, ensure_ascii=False)

    print(f"\n[DONE] Success: {success_count}, Failed: {fail_count}")

if __name__ == "__main__":
    main()
