"""
LiblibAI 美术资源生成器
用于生成魔兽归来游戏所需的美术资源

API文档参考: https://jishuzhan.net/article/1923220890280644610
"""

import requests
import json
import hmac
import base64
import time
import uuid
import os
from hashlib import sha1

# API密钥配置
ACCESS_KEY = 'r0LTLSLDq4e3a-PFdmrBpA'
SECRET_KEY = '4lH_K_Ya66ENhg-h_C64YnpnZdUoU6iJ'

# API端点
BASE_URL = 'https://openapi.liblibai.cloud'
TEXT2IMG_URI = '/api/generate/webui/text2img'
STATUS_URI = '/api/generate/webui/status'

# 输出目录
OUTPUT_DIR = os.path.join(os.path.dirname(__file__), '..', 'Assets', 'Resources', 'Sprites', 'Generated')

def make_sign(uri):
    """生成API签名"""
    timestamp = str(int(time.time() * 1000))
    signature_nonce = str(uuid.uuid4()).replace('-', '')
    content = '&'.join((uri, timestamp, signature_nonce))

    digest = hmac.new(SECRET_KEY.encode(), content.encode(), sha1).digest()
    sign = base64.urlsafe_b64encode(digest).rstrip(b'=').decode()
    return sign, timestamp, signature_nonce

def build_url(uri):
    """构建带签名的完整URL"""
    sign, timestamp, nonce = make_sign(uri)
    url = f'{BASE_URL}{uri}?AccessKey={ACCESS_KEY}&Signature={sign}&Timestamp={timestamp}&SignatureNonce={nonce}'
    return url

def text2img(prompt, width=1024, height=1024, steps=20, negative_prompt=""):
    """
    文生图接口

    Args:
        prompt: 正向提示词
        width: 图片宽度
        height: 图片高度
        steps: 生成步数
        negative_prompt: 负向提示词

    Returns:
        generateUuid: 生成任务ID
    """
    url = build_url(TEXT2IMG_URI)

    payload = {
        "templateUuid": "e10adc3949ba59abbe56e057f20f883e",  # 通用模板
        "generateParams": {
            "prompt": prompt,
            "negativePrompt": negative_prompt or "blurry, low quality, distorted, ugly, bad anatomy",
            "steps": steps,
            "width": width,
            "height": height,
            "cfgScale": 7,
            "sampler": "DPM++ 2M Karras",
            "seed": -1
        }
    }

    headers = {'Content-Type': 'application/json'}
    response = requests.post(url, headers=headers, data=json.dumps(payload))
    result = response.json()

    if result.get('code') == 0:
        return result['data']['generateUuid']
    else:
        print(f"Error: {result}")
        return None

def check_status(generate_uuid):
    """
    查询生成状态

    Args:
        generate_uuid: 生成任务ID

    Returns:
        dict: 包含状态和图片URL的结果
    """
    url = build_url(STATUS_URI)

    payload = {
        "generateUuid": generate_uuid
    }

    headers = {'Content-Type': 'application/json'}
    response = requests.post(url, headers=headers, data=json.dumps(payload))
    return response.json()

def download_image(image_url, save_path):
    """下载图片到本地"""
    response = requests.get(image_url)
    if response.status_code == 200:
        os.makedirs(os.path.dirname(save_path), exist_ok=True)
        with open(save_path, 'wb') as f:
            f.write(response.content)
        print(f"Downloaded: {save_path}")
        return True
    return False

def wait_for_completion(generate_uuid, max_wait=120):
    """等待生成完成"""
    print(f"Waiting for generation {generate_uuid}...")
    start_time = time.time()

    while time.time() - start_time < max_wait:
        result = check_status(generate_uuid)

        if result.get('code') == 0:
            data = result.get('data', {})
            status = data.get('generateStatus')

            if status == 5:  # 完成
                images = data.get('images', [])
                if images:
                    return images[0].get('imageUrl')
            elif status == -1:  # 失败
                print(f"Generation failed: {data}")
                return None

        time.sleep(3)

    print("Timeout waiting for generation")
    return None

def generate_and_save(prompt, filename, width=1024, height=1024):
    """生成图片并保存"""
    print(f"\n=== Generating: {filename} ===")
    print(f"Prompt: {prompt[:50]}...")

    # 生成
    uuid = text2img(prompt, width, height)
    if not uuid:
        return False

    # 等待完成
    image_url = wait_for_completion(uuid)
    if not image_url:
        return False

    # 下载
    save_path = os.path.join(OUTPUT_DIR, filename)
    return download_image(image_url, save_path)


# ============ 游戏资源生成任务 ============

# 地形纹理
TERRAIN_TEXTURES = [
    {
        "name": "Ground_Grass_01.png",
        "prompt": "seamless grass texture, top-down view, game asset, green grass field, high detail, tileable pattern, 2D game texture, clean edges",
        "width": 512,
        "height": 512
    },
    {
        "name": "Ground_Dirt_01.png",
        "prompt": "seamless dirt texture, top-down view, game asset, brown earth soil, high detail, tileable pattern, 2D game texture",
        "width": 512,
        "height": 512
    },
    {
        "name": "Ground_Stone_01.png",
        "prompt": "seamless stone floor texture, top-down view, game asset, grey cobblestone, medieval style, tileable pattern, 2D game texture",
        "width": 512,
        "height": 512
    }
]

# 特效贴图
VFX_TEXTURES = [
    {
        "name": "VFX_Arrow_Trail.png",
        "prompt": "arrow trail effect, glowing yellow light streak, transparent background, game VFX asset, soft glow, horizontal motion blur",
        "width": 256,
        "height": 64
    },
    {
        "name": "VFX_Skill_Glow.png",
        "prompt": "magical glow effect, circular radial light, golden orange color, transparent background, game skill effect, soft edges",
        "width": 256,
        "height": 256
    },
    {
        "name": "VFX_Hit_Spark.png",
        "prompt": "hit spark effect, impact flash, white yellow sparks, transparent background, game VFX, action game effect",
        "width": 128,
        "height": 128
    }
]

# UI元素
UI_ELEMENTS = [
    {
        "name": "UI_HealthBar_Frame.png",
        "prompt": "game health bar frame, medieval fantasy style, ornate border, golden brown frame, horizontal bar design, UI element, transparent background",
        "width": 256,
        "height": 64
    },
    {
        "name": "UI_Joystick_Base.png",
        "prompt": "virtual joystick base, circular dark grey disc, subtle glow, mobile game UI, clean modern design, transparent background",
        "width": 200,
        "height": 200
    },
    {
        "name": "UI_Joystick_Knob.png",
        "prompt": "virtual joystick knob, small white circle, soft shadow, mobile game UI control, clean design, transparent background",
        "width": 80,
        "height": 80
    }
]

def generate_all_assets():
    """生成所有游戏资源"""
    print("=" * 50)
    print("LiblibAI 游戏美术资源生成器")
    print("=" * 50)

    all_assets = TERRAIN_TEXTURES + VFX_TEXTURES + UI_ELEMENTS

    success_count = 0
    for asset in all_assets:
        if generate_and_save(
            prompt=asset["prompt"],
            filename=asset["name"],
            width=asset["width"],
            height=asset["height"]
        ):
            success_count += 1
        time.sleep(2)  # 避免请求过快

    print(f"\n=== Complete ===")
    print(f"Generated {success_count}/{len(all_assets)} assets")
    print(f"Output directory: {OUTPUT_DIR}")

if __name__ == "__main__":
    generate_all_assets()
