#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
sprite_cli.py - AI Asset Generation CLI Tool

CLI wrapper for LiblibAI (UI image generation) and Tripo3D (3D model generation).
Designed for AI agents to invoke directly via Bash.

Output convention:
  - stdout: structured JSON result (machine-readable)
  - stderr: progress logs with timestamps (human-readable)
  - exit code: 0 = success, 1 = failure

Usage examples:
  # Single UI image
  python sprite_cli.py ui --prompt "health bar background" --width 300 --height 40 -o bar.png

  # 3D model
  python sprite_cli.py model --prompt "wooden treasure chest" -o chest.glb

  # Batch from spec file
  python sprite_cli.py batch --spec ui_spec.json --skip-existing

  # Check task status
  python sprite_cli.py status --service liblib --task-id "uuid..."
"""
import argparse
import base64
import hashlib
import hmac
import json
import os
import random
import string
import sys
import time
import urllib.error
import urllib.parse
import urllib.request
from pathlib import Path


# Constants & Defaults

LIBLIB_BASE = "https://openapi.liblibai.cloud"
LIBLIB_TEMPLATE = "6f7c4652458d4802969f8d089cf5b91f"

TRIPO_BASE = "https://api.tripo3d.ai/v2/openapi"

BASE_PROMPT = (
    "World of Warcraft UI style, epic fantasy game UI, ornate metal and leather frame, "
    "embossed details, dramatic lighting, subtle wear and scratches, high detail, "
    "clean transparent background, no text, no watermark, no logo, no characters, no scenery"
)

DEFAULT_TIMEOUT = 900
DEFAULT_POLL_INTERVAL = 10
DEFAULT_SUBMIT_DELAY = 30
DEFAULT_MAX_RETRIES = 3
DEFAULT_RETRY_DELAY = 60
DEFAULT_GEN_WIDTH = 768
DEFAULT_GEN_HEIGHT = 1344
DEFAULT_STEPS = 20
DEFAULT_SEED = -1
DEFAULT_IMG_COUNT = 1

SCRIPT_DIR = Path(__file__).resolve().parent
DEFAULT_CONFIG_PATH = SCRIPT_DIR / "liblib_config.json"
DEFAULT_OUTPUT_DIR = SCRIPT_DIR / "output"


# Logging & Output Helpers

def log(message):
    """Print progress/status to stderr (keeps stdout clean for JSON)."""
    ts = time.strftime("%H:%M:%S")
    print("[%s] %s" % (ts, message), file=sys.stderr, flush=True)


def emit_result(result_dict):
    """Output final JSON result to stdout."""
    print(json.dumps(result_dict, ensure_ascii=False, indent=2), flush=True)


def default_output_path(prefix, ext):
    """Generate a default output file path with timestamp."""
    DEFAULT_OUTPUT_DIR.mkdir(parents=True, exist_ok=True)
    ts = time.strftime("%Y%m%d_%H%M%S")
    return str(DEFAULT_OUTPUT_DIR / ("%s_%s%s" % (prefix, ts, ext)))


# Credential Loading

def load_config(config_path=None, tripo_key_override=None):
    """
    Load credentials. Priority: CLI flag > env vars > config file.
    Returns dict with liblib_access_key, liblib_secret_key, tripo_api_key.
    """
    result = {
        "liblib_access_key": os.environ.get("LIBLIB_ACCESS_KEY", "").strip(),
        "liblib_secret_key": os.environ.get("LIBLIB_SECRET_KEY", "").strip(),
        "tripo_api_key": os.environ.get("TRIPO_API_KEY", "").strip(),
    }
    cfg_path = Path(config_path) if config_path else DEFAULT_CONFIG_PATH
    if cfg_path.exists():
        try:
            with open(cfg_path, "r", encoding="utf-8") as f:
                cfg = json.load(f)
            if not result["liblib_access_key"]:
                result["liblib_access_key"] = (cfg.get("access_key") or "").strip()
            if not result["liblib_secret_key"]:
                result["liblib_secret_key"] = (cfg.get("secret_key") or "").strip()
            if not result["tripo_api_key"]:
                result["tripo_api_key"] = (cfg.get("tripo_api_key") or "").strip()
        except Exception as e:
            log("WARNING: Failed to read config %s: %s" % (cfg_path, e))
    if tripo_key_override:
        result["tripo_api_key"] = tripo_key_override.strip()
    return result


# LiblibAI API Functions

def liblib_sign(secret_key, uri):
    """Generate HMAC-SHA1 signature for LiblibAI API."""
    ts = str(int(time.time() * 1000))
    nonce = "".join(random.choices(string.ascii_letters + string.digits, k=10))
    content = "{0}&{1}&{2}".format(uri, ts, nonce)
    sig = base64.urlsafe_b64encode(
        hmac.new(secret_key.encode(), content.encode(), hashlib.sha1).digest()
    ).decode().rstrip("=")
    return sig, ts, nonce


def liblib_text2img(access_key, secret_key, prompt, params=None):
    uri = "/api/generate/webui/text2img"
    sig, ts, nonce = liblib_sign(secret_key, uri)
    url = "{0}{1}?AccessKey={2}&Signature={3}&Timestamp={4}&SignatureNonce={5}".format(
        LIBLIB_BASE, uri,
        urllib.parse.quote(access_key), urllib.parse.quote(sig), ts, nonce
    )
    gen_params = {
        "prompt": prompt, "steps": DEFAULT_STEPS,
        "width": DEFAULT_GEN_WIDTH, "height": DEFAULT_GEN_HEIGHT,
        "imgCount": DEFAULT_IMG_COUNT, "seed": DEFAULT_SEED, "restoreFaces": 0,
    }
    if params:
        for k in ("steps", "width", "height", "imgCount", "seed", "restoreFaces"):
            if k in params:
                gen_params[k] = params[k]
    template_uuid = (params or {}).get("templateUuid", LIBLIB_TEMPLATE)
    body = json.dumps({"templateUuid": template_uuid, "generateParams": gen_params}).encode("utf-8")
    req = urllib.request.Request(url, data=body, method="POST")
    req.add_header("Content-Type", "application/json")
    try:
        with urllib.request.urlopen(req, timeout=60) as resp:
            data = json.loads(resp.read().decode())
            if data.get("code") == 0 and data.get("data"):
                uuid = data["data"].get("generateUuid")
                if uuid:
                    return uuid
            raise RuntimeError("LiblibAI submit failed: %s" % json.dumps(data, ensure_ascii=False))
    except urllib.error.URLError as e:
        raise RuntimeError("LiblibAI network error: %s" % e)


def liblib_status(access_key, secret_key, uuid):
    uri = "/api/generate/webui/status"
    sig, ts, nonce = liblib_sign(secret_key, uri)
    url = "{0}{1}?AccessKey={2}&Signature={3}&Timestamp={4}&SignatureNonce={5}".format(
        LIBLIB_BASE, uri,
        urllib.parse.quote(access_key), urllib.parse.quote(sig), ts, nonce
    )
    body = json.dumps({"generateUuid": uuid}).encode("utf-8")
    req = urllib.request.Request(url, data=body, method="POST")
    req.add_header("Content-Type", "application/json")
    try:
        with urllib.request.urlopen(req, timeout=30) as resp:
            data = json.loads(resp.read().decode())
            if data.get("code") == 0:
                d = data.get("data", {})
                status = d.get("generateStatus")
                if status in ("SUCCEED", "SUCCESS", 5, "5"):
                    imgs = d.get("images", [])
                    image_url = ""
                    if imgs:
                        first = imgs[0]
                        if isinstance(first, dict):
                            image_url = first.get("imageUrl", "") or first.get("url", "")
                        else:
                            image_url = first
                    return {"status": "success", "image_url": image_url}
                if status in ("FAILED", 4, "4"):
                    err = d.get("failReason") or d.get("message") or "unknown failure"
                    return {"status": "failed", "error": err}
                return {"status": "processing"}
        return {"status": "error", "error": "invalid response"}
    except urllib.error.URLError as e:
        return {"status": "error", "error": "network error: %s" % e}


def liblib_generate_and_wait(access_key, secret_key, prompt, params=None,
                             timeout=DEFAULT_TIMEOUT, poll_interval=DEFAULT_POLL_INTERVAL):
    start = time.time()
    try:
        task_id = liblib_text2img(access_key, secret_key, prompt, params)
    except RuntimeError as e:
        return {"ok": False, "task_id": None, "status": "submit_failed",
                "error": str(e), "elapsed_seconds": round(time.time() - start, 1)}
    log("Task submitted: %s" % task_id)
    while True:
        elapsed = time.time() - start
        if elapsed > timeout:
            return {"ok": False, "task_id": task_id, "status": "timeout",
                    "error": "timeout after %ds" % int(elapsed), "elapsed_seconds": round(elapsed, 1)}
        time.sleep(poll_interval)
        result = liblib_status(access_key, secret_key, task_id)
        if result["status"] == "success":
            return {"ok": True, "task_id": task_id, "status": "success",
                    "image_url": result.get("image_url", ""), "elapsed_seconds": round(time.time() - start, 1)}
        if result["status"] in ("failed", "error"):
            return {"ok": False, "task_id": task_id, "status": result["status"],
                    "error": result.get("error", "unknown"), "elapsed_seconds": round(time.time() - start, 1)}
        log("Polling... elapsed=%ds status=%s" % (int(elapsed), result["status"]))


# Tripo3D API Functions

def tripo_create_task(api_key, task_type, prompt=None, image_url=None):
    payload = {"type": task_type}
    if task_type == "text_to_model" and prompt:
        payload["prompt"] = prompt
    elif task_type == "image_to_model" and image_url:
        payload["image_url"] = image_url
    else:
        raise RuntimeError("Invalid task_type or missing prompt/image_url")
    body = json.dumps(payload).encode("utf-8")
    req = urllib.request.Request(TRIPO_BASE + "/task", data=body, method="POST")
    req.add_header("Content-Type", "application/json")
    req.add_header("Authorization", "Bearer " + api_key)
    try:
        with urllib.request.urlopen(req, timeout=60) as resp:
            data = json.loads(resp.read().decode())
            tid = data.get("data", {}).get("task_id") or data.get("task_id")
            if tid:
                return tid
            raise RuntimeError("Tripo3D submit failed: %s" % json.dumps(data, ensure_ascii=False))
    except urllib.error.URLError as e:
        raise RuntimeError("Tripo3D network error: %s" % e)


def tripo_get_task(api_key, task_id):
    req = urllib.request.Request(TRIPO_BASE + "/task/" + task_id)
    req.add_header("Authorization", "Bearer " + api_key)
    try:
        with urllib.request.urlopen(req, timeout=30) as resp:
            data = json.loads(resp.read().decode())
            task = data.get("data") or data
            status = (task.get("status") or "").lower()
            output = task.get("output", {}) or {}
            result = task.get("result", {}) or {}
            model_url = output.get("model") or output.get("model_mesh") or output.get("pbr_model")
            if not model_url and result:
                pbr = result.get("pbr_model") or result.get("model")
                if isinstance(pbr, dict):
                    model_url = pbr.get("url", "")
                else:
                    model_url = pbr
            if isinstance(model_url, dict):
                model_url = model_url.get("url", "")
            else:
                model_url = str(model_url) if model_url else ""
            return {"status": status, "model_url": model_url}
    except urllib.error.URLError as e:
        return {"status": "error", "error": "network error: %s" % e}


def tripo_generate_and_wait(api_key, task_type, prompt=None, image_url=None,
                            timeout=600, poll_interval=DEFAULT_POLL_INTERVAL):
    start = time.time()
    try:
        task_id = tripo_create_task(api_key, task_type, prompt, image_url)
    except RuntimeError as e:
        return {"ok": False, "task_id": None, "status": "submit_failed",
                "error": str(e), "elapsed_seconds": round(time.time() - start, 1)}
    log("Tripo3D task submitted: %s" % task_id)
    while True:
        elapsed = time.time() - start
        if elapsed > timeout:
            return {"ok": False, "task_id": task_id, "status": "timeout",
                    "error": "timeout after %ds" % int(elapsed), "elapsed_seconds": round(elapsed, 1)}
        time.sleep(poll_interval)
        result = tripo_get_task(api_key, task_id)
        if result["status"] == "success" and result.get("model_url"):
            return {"ok": True, "task_id": task_id, "status": "success",
                    "model_url": result["model_url"], "elapsed_seconds": round(time.time() - start, 1)}
        if result["status"] in ("failed", "error"):
            return {"ok": False, "task_id": task_id, "status": result["status"],
                    "error": result.get("error", "unknown"), "elapsed_seconds": round(time.time() - start, 1)}
        log("Polling Tripo3D... elapsed=%ds status=%s" % (int(elapsed), result["status"]))


# File Operations

def download_file(url, output_path):
    output_path = Path(output_path)
    output_path.parent.mkdir(parents=True, exist_ok=True)
    try:
        with urllib.request.urlopen(url, timeout=120) as resp:
            data = resp.read()
        with open(output_path, "wb") as f:
            f.write(data)
        log("Downloaded: %s (%d bytes)" % (output_path, len(data)))
        return output_path
    except Exception as e:
        raise RuntimeError("Download failed (%s): %s" % (url, e))


def resize_image(input_path, width, height):
    try:
        from PIL import Image
    except ImportError:
        log("WARNING: Pillow not installed, skipping resize. Install with: pip install Pillow")
        return input_path
    input_path = Path(input_path)
    img = Image.open(input_path).convert("RGBA")
    img = img.resize((width, height), Image.LANCZOS)
    out_path = input_path.with_suffix(".png")
    img.save(out_path)
    if out_path != input_path:
        input_path.unlink(missing_ok=True)
    log("Resized to %dx%d: %s" % (width, height, out_path))
    return out_path


# Subcommand: ui

def cmd_ui(args):
    creds = load_config(args.config)
    if not creds["liblib_access_key"] or not creds["liblib_secret_key"]:
        emit_result({"ok": False, "command": "ui", "error": "Missing LiblibAI credentials."})
        return 1
    prefix = args.style_prefix if args.style_prefix is not None else BASE_PROMPT
    full_prompt = (prefix + ", " + args.prompt) if prefix else args.prompt
    params = {"steps": args.steps, "width": args.gen_width, "height": args.gen_height,
              "imgCount": args.img_count, "seed": args.seed}
    if args.template_uuid:
        params["templateUuid"] = args.template_uuid
    log("Generating UI image...")
    log("Prompt: %s" % full_prompt)
    result = liblib_generate_and_wait(
        creds["liblib_access_key"], creds["liblib_secret_key"],
        full_prompt, params, args.timeout, args.poll_interval
    )
    if result["ok"] and not args.no_download:
        output_path = args.output or default_output_path("ui", ".png")
        try:
            download_file(result["image_url"], output_path)
            if args.width and args.height and not args.no_resize:
                resize_image(output_path, args.width, args.height)
            result["output_path"] = str(Path(output_path).resolve())
        except RuntimeError as e:
            result["ok"] = False
            result["error"] = str(e)
    if args.width: result["width"] = args.width
    if args.height: result["height"] = args.height
    result["command"] = "ui"
    result["prompt"] = full_prompt
    emit_result(result)
    return 0 if result["ok"] else 1


# Subcommand: model

def cmd_model(args):
    creds = load_config(args.config, tripo_key_override=args.tripo_key)
    if not creds["tripo_api_key"]:
        emit_result({"ok": False, "command": "model", "error": "Missing Tripo3D API key."})
        return 1
    task_type = args.type
    if task_type == "text_to_model" and not args.prompt:
        emit_result({"ok": False, "command": "model", "error": "text_to_model requires --prompt"})
        return 1
    if task_type == "image_to_model" and not args.image_url:
        emit_result({"ok": False, "command": "model", "error": "image_to_model requires --image-url"})
        return 1
    log("Generating 3D model (%s)..." % task_type)
    result = tripo_generate_and_wait(
        creds["tripo_api_key"], task_type,
        prompt=args.prompt, image_url=args.image_url,
        timeout=args.timeout, poll_interval=args.poll_interval
    )
    if result["ok"] and not args.no_download:
        output_path = args.output or default_output_path("model", ".glb")
        try:
            download_file(result["model_url"], output_path)
            result["output_path"] = str(Path(output_path).resolve())
        except RuntimeError as e:
            result["ok"] = False
            result["error"] = str(e)
    result["command"] = "model"
    result["type"] = task_type
    emit_result(result)
    return 0 if result["ok"] else 1


# Subcommand: batch

def cmd_batch(args):
    creds = load_config(args.config)
    if not creds["liblib_access_key"] or not creds["liblib_secret_key"]:
        emit_result({"ok": False, "command": "batch", "error": "Missing LiblibAI credentials."})
        return 1
    try:
        with open(args.spec, "r", encoding="utf-8") as f:
            spec = json.load(f)
    except Exception as e:
        emit_result({"ok": False, "command": "batch", "error": "Failed to load spec: %s" % e})
        return 1
    items = spec.get("items", [])
    if not items:
        emit_result({"ok": False, "command": "batch", "error": "No items in spec."})
        return 1
    if args.style_prefix is not None:
        style_prefix = args.style_prefix
    else:
        style_prefix = spec.get("style_prefix", BASE_PROMPT)
    output_root = Path(args.output_root or spec.get("output_root", str(DEFAULT_OUTPUT_DIR)))
    if args.dry_run:
        plan = {"ok": True, "command": "batch", "dry_run": True, "total": len(items),
                "style_prefix": style_prefix, "output_root": str(output_root), "items": []}
        for item in items:
            out_path = output_root / item.get("category", "") / item.get("filename", "")
            plan["items"].append({
                "category": item.get("category", ""), "filename": item.get("filename", ""),
                "width": item.get("width"), "height": item.get("height"),
                "prompt_preview": (style_prefix + ", " + item.get("prompt", ""))[:120] + "...",
                "output_path": str(out_path), "exists": out_path.exists(),
            })
        emit_result(plan)
        return 0
    results = []
    succeeded = failed = skipped = 0
    start_time = time.time()
    total = len(items)
    log("Batch generation started: %d items" % total)
    for idx, item in enumerate(items, 1):
        category = item.get("category", "")
        filename = item.get("filename", "unknown.png")
        w, h = item.get("width"), item.get("height")
        detail_prompt = item.get("prompt", "")
        out_path = output_root / category / filename
        if args.skip_existing and out_path.exists():
            log("[%d/%d] SKIP (exists): %s" % (idx, total, out_path))
            results.append({"filename": filename, "category": category, "status": "skipped", "output_path": str(out_path)})
            skipped += 1
            continue
        full_prompt = (style_prefix + ", " + detail_prompt) if style_prefix else detail_prompt
        if idx > 1 and args.submit_delay > 0:
            log("Waiting %ds..." % args.submit_delay)
            time.sleep(args.submit_delay)
        log("[%d/%d] Generating: %s/%s" % (idx, total, category, filename))
        item_result = None
        last_error = "unknown"
        for attempt in range(1, args.max_retries + 1):
            try:
                gen_result = liblib_generate_and_wait(
                    creds["liblib_access_key"], creds["liblib_secret_key"],
                    full_prompt, None, args.timeout, args.poll_interval
                )
                if gen_result["ok"]:
                    download_file(gen_result["image_url"], out_path)
                    if w and h: resize_image(out_path, w, h)
                    item_result = {"filename": filename, "category": category, "status": "success",
                                   "output_path": str(out_path), "image_url": gen_result.get("image_url", ""),
                                   "task_id": gen_result.get("task_id", "")}
                    succeeded += 1
                    log("[OK] Saved: %s" % out_path)
                    break
                else:
                    last_error = gen_result.get("error", "unknown")
                    log("[WARN] Attempt %d/%d failed: %s" % (attempt, args.max_retries, last_error))
            except Exception as e:
                last_error = str(e)
                log("[WARN] Attempt %d/%d exception: %s" % (attempt, args.max_retries, last_error))
            if attempt < args.max_retries:
                log("Retrying in %ds..." % args.retry_delay)
                time.sleep(args.retry_delay)
        if item_result is None:
            item_result = {"filename": filename, "category": category, "status": "failed", "error": last_error}
            failed += 1
        results.append(item_result)
    elapsed = round(time.time() - start_time, 1)
    log("Batch complete: %d succeeded, %d skipped, %d failed (%.1fs)" % (succeeded, skipped, failed, elapsed))
    emit_result({"ok": failed == 0, "command": "batch", "total": total,
                 "succeeded": succeeded, "skipped": skipped, "failed": failed,
                 "elapsed_seconds": elapsed, "results": results})
    return 0 if failed == 0 else 1


# Subcommand: status

def cmd_status(args):
    creds = load_config(args.config, tripo_key_override=args.tripo_key)
    if args.service == "liblib":
        if not creds["liblib_access_key"] or not creds["liblib_secret_key"]:
            emit_result({"ok": False, "command": "status", "error": "Missing LiblibAI credentials."})
            return 1
        result = liblib_status(creds["liblib_access_key"], creds["liblib_secret_key"], args.task_id)
        result.update({"command": "status", "service": "liblib", "task_id": args.task_id, "ok": result.get("status") != "error"})
        emit_result(result)
        return 0 if result["ok"] else 1
    elif args.service == "tripo":
        if not creds["tripo_api_key"]:
            emit_result({"ok": False, "command": "status", "error": "Missing Tripo3D API key."})
            return 1
        result = tripo_get_task(creds["tripo_api_key"], args.task_id)
        result.update({"command": "status", "service": "tripo", "task_id": args.task_id, "ok": result.get("status") not in ("error", "failed")})
        emit_result(result)
        return 0 if result["ok"] else 1
    else:
        emit_result({"ok": False, "command": "status", "error": "Unknown service: %s" % args.service})
        return 1


# Argparse & Main

def build_parser():
    parser = argparse.ArgumentParser(prog="sprite_cli",
        description="AI Asset Generation CLI - LiblibAI (UI) + Tripo3D (3D). JSON stdout, logs stderr.",
        formatter_class=argparse.RawDescriptionHelpFormatter)
    sub = parser.add_subparsers(dest="command", required=True, help="Available commands")
    # ui
    p_ui = sub.add_parser("ui", help="Generate single UI image via LiblibAI")
    p_ui.add_argument("--prompt", required=True)
    p_ui.add_argument("--width", type=int, default=None)
    p_ui.add_argument("--height", type=int, default=None)
    p_ui.add_argument("-o", "--output", default=None)
    p_ui.add_argument("--style-prefix", default=None)
    p_ui.add_argument("--template-uuid", default=LIBLIB_TEMPLATE)
    p_ui.add_argument("--steps", type=int, default=DEFAULT_STEPS)
    p_ui.add_argument("--seed", type=int, default=DEFAULT_SEED)
    p_ui.add_argument("--img-count", type=int, default=DEFAULT_IMG_COUNT)
    p_ui.add_argument("--gen-width", type=int, default=DEFAULT_GEN_WIDTH)
    p_ui.add_argument("--gen-height", type=int, default=DEFAULT_GEN_HEIGHT)
    p_ui.add_argument("--timeout", type=int, default=DEFAULT_TIMEOUT)
    p_ui.add_argument("--poll-interval", type=int, default=DEFAULT_POLL_INTERVAL)
    p_ui.add_argument("-c", "--config", default=None)
    p_ui.add_argument("--no-resize", action="store_true")
    p_ui.add_argument("--no-download", action="store_true")
    # model
    p_model = sub.add_parser("model", help="Generate 3D model via Tripo3D")
    p_model.add_argument("--type", default="text_to_model", choices=["text_to_model", "image_to_model"])
    p_model.add_argument("--prompt", default=None)
    p_model.add_argument("--image-url", default=None)
    p_model.add_argument("-o", "--output", default=None)
    p_model.add_argument("--timeout", type=int, default=600)
    p_model.add_argument("--poll-interval", type=int, default=DEFAULT_POLL_INTERVAL)
    p_model.add_argument("--tripo-key", default=None)
    p_model.add_argument("-c", "--config", default=None)
    p_model.add_argument("--no-download", action="store_true")
    # batch
    p_batch = sub.add_parser("batch", help="Batch generate UI images from JSON spec")
    p_batch.add_argument("-s", "--spec", required=True)
    p_batch.add_argument("--output-root", default=None)
    p_batch.add_argument("--style-prefix", default=None)
    p_batch.add_argument("--submit-delay", type=int, default=DEFAULT_SUBMIT_DELAY)
    p_batch.add_argument("--max-retries", type=int, default=DEFAULT_MAX_RETRIES)
    p_batch.add_argument("--retry-delay", type=int, default=DEFAULT_RETRY_DELAY)
    p_batch.add_argument("--timeout", type=int, default=DEFAULT_TIMEOUT)
    p_batch.add_argument("--poll-interval", type=int, default=DEFAULT_POLL_INTERVAL)
    p_batch.add_argument("--skip-existing", action="store_true")
    p_batch.add_argument("-c", "--config", default=None)
    p_batch.add_argument("--dry-run", action="store_true")
    # status
    p_status = sub.add_parser("status", help="Query task status")
    p_status.add_argument("--service", required=True, choices=["liblib", "tripo"])
    p_status.add_argument("--task-id", required=True)
    p_status.add_argument("--tripo-key", default=None)
    p_status.add_argument("-c", "--config", default=None)
    return parser


def main():
    parser = build_parser()
    args = parser.parse_args()
    try:
        if args.command == "ui": exit_code = cmd_ui(args)
        elif args.command == "model": exit_code = cmd_model(args)
        elif args.command == "batch": exit_code = cmd_batch(args)
        elif args.command == "status": exit_code = cmd_status(args)
        else:
            emit_result({"ok": False, "command": args.command, "error": "Unknown command"})
            exit_code = 1
    except KeyboardInterrupt:
        log("Interrupted")
        emit_result({"ok": False, "command": args.command, "error": "interrupted"})
        exit_code = 130
    except Exception as e:
        log("Unexpected error: %s" % e)
        emit_result({"ok": False, "command": getattr(args, "command", "unknown"), "error": "unexpected: %s" % e})
        exit_code = 1
    sys.exit(exit_code)


if __name__ == "__main__":
    main()
