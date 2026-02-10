#!/usr/bin/env node
/**
 * UI Mockup Batch Generator for LiblibAI
 * Node.js version of sprite_cli.py batch command
 */

const https = require('https');
const http = require('http');
const fs = require('fs');
const path = require('path');
const crypto = require('crypto');

// Configuration
const config = JSON.parse(fs.readFileSync(path.join(__dirname, 'liblib_config.json'), 'utf8'));
const ACCESS_KEY = config.access_key;
const SECRET_KEY = config.secret_key;

const LIBLIB_BASE = 'https://openapi.liblibai.cloud';
const LIBLIB_TEMPLATE = '6f7c4652458d4802969f8d089cf5b91f';

// Generation parameters
const DEFAULT_TIMEOUT = 900000; // 15 minutes
const DEFAULT_POLL_INTERVAL = 10000; // 10 seconds
const DEFAULT_SUBMIT_DELAY = 30000; // 30 seconds between requests
const DEFAULT_GEN_WIDTH = 768;
const DEFAULT_GEN_HEIGHT = 1344;
const DEFAULT_STEPS = 20;

function log(message) {
    const ts = new Date().toLocaleTimeString('zh-CN', { hour12: false });
    console.error(`[${ts}] ${message}`);
}

function makeSign(uri) {
    const timestamp = Date.now().toString();
    const nonce = crypto.randomBytes(5).toString('hex');
    const content = `${uri}&${timestamp}&${nonce}`;
    const signature = crypto.createHmac('sha1', SECRET_KEY)
        .update(content)
        .digest('base64')
        .replace(/\+/g, '-')
        .replace(/\//g, '_')
        .replace(/=+$/, '');
    return { signature, timestamp, nonce };
}

function httpsRequest(options, postData = null) {
    return new Promise((resolve, reject) => {
        const protocol = options.protocol === 'http:' ? http : https;
        const req = protocol.request(options, (res) => {
            let data = '';
            res.on('data', chunk => data += chunk);
            res.on('end', () => {
                try {
                    if (options.binary) {
                        resolve({ statusCode: res.statusCode, data: Buffer.from(data, 'binary') });
                    } else {
                        resolve({ statusCode: res.statusCode, data: JSON.parse(data) });
                    }
                } catch (e) {
                    resolve({ statusCode: res.statusCode, data: data });
                }
            });
        });
        req.on('error', reject);
        req.setTimeout(60000, () => {
            req.destroy();
            reject(new Error('Request timeout'));
        });
        if (postData) req.write(postData);
        req.end();
    });
}

async function submitText2Img(prompt, params = {}) {
    const uri = '/api/generate/webui/text2img';
    const { signature, timestamp, nonce } = makeSign(uri);

    const genParams = {
        prompt: prompt,
        steps: params.steps || DEFAULT_STEPS,
        width: params.width || DEFAULT_GEN_WIDTH,
        height: params.height || DEFAULT_GEN_HEIGHT,
        imgCount: 1,
        seed: -1,
        restoreFaces: 0
    };

    const body = JSON.stringify({
        templateUuid: LIBLIB_TEMPLATE,
        generateParams: genParams
    });

    const url = new URL(LIBLIB_BASE + uri);
    url.searchParams.set('AccessKey', ACCESS_KEY);
    url.searchParams.set('Signature', signature);
    url.searchParams.set('Timestamp', timestamp);
    url.searchParams.set('SignatureNonce', nonce);

    const options = {
        hostname: url.hostname,
        port: 443,
        path: url.pathname + url.search,
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'Content-Length': Buffer.byteLength(body)
        }
    };

    const response = await httpsRequest(options, body);
    if (response.data.code === 0 && response.data.data?.generateUuid) {
        return response.data.data.generateUuid;
    }
    throw new Error(`Submit failed: ${JSON.stringify(response.data)}`);
}

async function checkStatus(uuid) {
    const uri = '/api/generate/webui/status';
    const { signature, timestamp, nonce } = makeSign(uri);

    const body = JSON.stringify({ generateUuid: uuid });

    const url = new URL(LIBLIB_BASE + uri);
    url.searchParams.set('AccessKey', ACCESS_KEY);
    url.searchParams.set('Signature', signature);
    url.searchParams.set('Timestamp', timestamp);
    url.searchParams.set('SignatureNonce', nonce);

    const options = {
        hostname: url.hostname,
        port: 443,
        path: url.pathname + url.search,
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'Content-Length': Buffer.byteLength(body)
        }
    };

    const response = await httpsRequest(options, body);
    if (response.data.code === 0) {
        const d = response.data.data || {};
        const status = d.generateStatus;

        if (['SUCCEED', 'SUCCESS', 5, '5'].includes(status)) {
            const images = d.images || [];
            let imageUrl = '';
            if (images.length > 0) {
                const first = images[0];
                imageUrl = typeof first === 'object' ? (first.imageUrl || first.url || '') : first;
            }
            return { status: 'success', imageUrl };
        }
        if (['FAILED', 4, '4'].includes(status)) {
            return { status: 'failed', error: d.failReason || d.message || 'unknown' };
        }
        return { status: 'processing' };
    }
    return { status: 'error', error: 'Invalid response' };
}

async function downloadFile(url, outputPath) {
    return new Promise((resolve, reject) => {
        const urlObj = new URL(url);
        const protocol = urlObj.protocol === 'http:' ? http : https;

        const request = protocol.get(url, (response) => {
            if (response.statusCode >= 300 && response.statusCode < 400 && response.headers.location) {
                // Follow redirect
                downloadFile(response.headers.location, outputPath).then(resolve).catch(reject);
                return;
            }

            const dir = path.dirname(outputPath);
            if (!fs.existsSync(dir)) {
                fs.mkdirSync(dir, { recursive: true });
            }

            const file = fs.createWriteStream(outputPath);
            response.pipe(file);
            file.on('finish', () => {
                file.close();
                const stats = fs.statSync(outputPath);
                log(`Downloaded: ${outputPath} (${stats.size} bytes)`);
                resolve(outputPath);
            });
        });

        request.on('error', reject);
        request.setTimeout(120000, () => {
            request.destroy();
            reject(new Error('Download timeout'));
        });
    });
}

async function generateAndWait(prompt, params = {}, timeout = DEFAULT_TIMEOUT) {
    const startTime = Date.now();

    try {
        const taskId = await submitText2Img(prompt, params);
        log(`Task submitted: ${taskId}`);

        while (true) {
            const elapsed = Date.now() - startTime;
            if (elapsed > timeout) {
                return { ok: false, taskId, status: 'timeout', error: `Timeout after ${Math.round(elapsed/1000)}s` };
            }

            await new Promise(r => setTimeout(r, DEFAULT_POLL_INTERVAL));

            const result = await checkStatus(taskId);

            if (result.status === 'success') {
                return { ok: true, taskId, status: 'success', imageUrl: result.imageUrl };
            }
            if (['failed', 'error'].includes(result.status)) {
                return { ok: false, taskId, status: result.status, error: result.error || 'unknown' };
            }

            log(`Polling... elapsed=${Math.round(elapsed/1000)}s status=${result.status}`);
        }
    } catch (e) {
        return { ok: false, taskId: null, status: 'error', error: e.message };
    }
}

async function runBatch(specPath, options = {}) {
    const spec = JSON.parse(fs.readFileSync(specPath, 'utf8'));
    const items = spec.items || [];
    const stylePrefix = options.stylePrefix || spec.style_prefix || '';
    const outputRoot = options.outputRoot || spec.output_root || __dirname;
    const skipExisting = options.skipExisting || false;
    const submitDelay = options.submitDelay || DEFAULT_SUBMIT_DELAY;

    const results = [];
    let succeeded = 0, failed = 0, skipped = 0;
    const total = items.length;

    log(`Batch generation started: ${total} items`);
    log(`Output root: ${outputRoot}`);
    log(`Style prefix: ${stylePrefix.substring(0, 80)}...`);

    for (let i = 0; i < items.length; i++) {
        const item = items[i];
        const category = item.category || '';
        const filename = item.filename || 'unknown.png';
        const detailPrompt = item.prompt || '';
        const outPath = path.join(outputRoot, category, filename);

        if (skipExisting && fs.existsSync(outPath)) {
            log(`[${i+1}/${total}] SKIP (exists): ${outPath}`);
            results.push({ filename, category, status: 'skipped', outputPath: outPath });
            skipped++;
            continue;
        }

        const fullPrompt = stylePrefix ? `${stylePrefix}, ${detailPrompt}` : detailPrompt;

        if (i > 0 && submitDelay > 0) {
            log(`Waiting ${submitDelay/1000}s...`);
            await new Promise(r => setTimeout(r, submitDelay));
        }

        log(`[${i+1}/${total}] Generating: ${category}/${filename}`);

        const genResult = await generateAndWait(fullPrompt);

        if (genResult.ok && genResult.imageUrl) {
            try {
                await downloadFile(genResult.imageUrl, outPath);
                results.push({
                    filename,
                    category,
                    status: 'success',
                    outputPath: outPath,
                    taskId: genResult.taskId
                });
                succeeded++;
                log(`[OK] Saved: ${outPath}`);
            } catch (e) {
                results.push({ filename, category, status: 'failed', error: e.message });
                failed++;
                log(`[FAIL] Download error: ${e.message}`);
            }
        } else {
            results.push({
                filename,
                category,
                status: 'failed',
                error: genResult.error || 'Generation failed'
            });
            failed++;
            log(`[FAIL] ${genResult.error}`);
        }
    }

    const elapsed = Math.round((Date.now() - Date.now()) / 1000);
    log(`Batch complete: ${succeeded} succeeded, ${skipped} skipped, ${failed} failed`);

    return {
        ok: failed === 0,
        total,
        succeeded,
        skipped,
        failed,
        results
    };
}

// Main
async function main() {
    const args = process.argv.slice(2);

    if (args.includes('--help') || args.includes('-h')) {
        console.log(`
UI Mockup Batch Generator

Usage:
  node batch_generate.js [options]

Options:
  --spec <file>     Spec JSON file (default: ui_mockups_spec.json)
  --skip-existing   Skip files that already exist
  --dry-run         Show what would be generated without actually generating
  --help            Show this help
        `);
        process.exit(0);
    }

    const specIndex = args.indexOf('--spec');
    const specPath = specIndex >= 0 ? args[specIndex + 1] : path.join(__dirname, 'ui_mockups_spec.json');
    const skipExisting = args.includes('--skip-existing');
    const dryRun = args.includes('--dry-run');

    if (!fs.existsSync(specPath)) {
        console.error(`Spec file not found: ${specPath}`);
        process.exit(1);
    }

    if (dryRun) {
        const spec = JSON.parse(fs.readFileSync(specPath, 'utf8'));
        console.log(JSON.stringify({
            ok: true,
            dryRun: true,
            total: spec.items.length,
            stylePrefix: spec.style_prefix?.substring(0, 100) + '...',
            outputRoot: spec.output_root,
            items: spec.items.map(item => ({
                category: item.category,
                filename: item.filename,
                width: item.width,
                height: item.height,
                exists: fs.existsSync(path.join(spec.output_root, item.category, item.filename))
            }))
        }, null, 2));
        process.exit(0);
    }

    try {
        const result = await runBatch(specPath, { skipExisting });
        console.log(JSON.stringify(result, null, 2));
        process.exit(result.ok ? 0 : 1);
    } catch (e) {
        console.error(`Error: ${e.message}`);
        process.exit(1);
    }
}

main();
