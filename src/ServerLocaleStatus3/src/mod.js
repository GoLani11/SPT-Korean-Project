"use strict";

const fs = require("fs");
const path = require("path");
const crypto = require("crypto");

function readJson(file) {
    return JSON.parse(fs.readFileSync(file, "utf8").replace(/^\uFEFF/, ""));
}

function checkHash(file, expected) {
    if (crypto.createHash("sha256").update(fs.readFileSync(file)).digest("hex") !== expected) {
        throw new Error(`파일 검증 실패: ${path.basename(file)}`);
    }
}

function eftVersion(file) {
    const bytes = fs.readFileSync(file);
    // Primary VS_FIXEDFILEINFO resource. EFT also retains a later Unity-player resource;
    // Windows FileVersionInfo selects the first one. Avoid localized/truncated strings.
    const signature = Buffer.from([0xbd, 0x04, 0xef, 0xfe, 0x00, 0x00, 0x01, 0x00]);
    const offset = bytes.indexOf(signature);
    if (offset < 0 || offset + 16 > bytes.length) {
        throw new Error("EFT 실행 파일의 버전 정보를 확인할 수 없습니다.");
    }
    return [bytes.readUInt16LE(offset + 10), bytes.readUInt16LE(offset + 8),
        bytes.readUInt16LE(offset + 14), bytes.readUInt16LE(offset + 12)].join(".");
}

function verify(root) {
    const core = ["Aki_Data", "SPT_Data"].map(dir => path.join(root, dir, "Server/configs/core.json"))
        .find(file => fs.existsSync(file));
    if (!core) throw new Error("SPT 서버 버전을 확인할 수 없습니다.");
    const config = readJson(core);
    const version = config.akiVersion || config.sptVersion;
    const plugins = path.join(root, "BepInEx/plugins");
    const bundle = path.join(plugins, "SPT-Korean");
    const manifest = readJson(path.join(bundle, "manifest.json"));
    const profile = manifest.profiles[version];
    if (manifest.schemaVersion !== 1 || !profile) throw new Error(`SPT ${version} 번역 프로필이 없습니다.`);
    if (manifest.clientVersion !== "2.2.0") throw new Error("알림 모듈과 클라이언트 버전이 다릅니다.");
    checkHash(path.join(plugins, "GoLani.KoreanModFix.dll"), manifest.clientDllSha256);
    const executable = path.join(root, "EscapeFromTarkov.exe");
    if (!fs.existsSync(executable)) throw new Error("EscapeFromTarkov.exe가 없습니다. 게임 파일 설치 후 테스트할 수 있습니다.");
    if (eftVersion(executable) !== profile.eftVersion) throw new Error("EFT 빌드가 번역 프로필과 다릅니다.");
    if (!/^\d+\.\d+\.\d+$/.test(profile.translationVersion)) throw new Error("번역 버전 형식 오류");
    const locales = {};
    for (const name of ["en.json", "kr.json", "kr-en.json"]) {
        const file = path.join(bundle, "locales", profile.translationVersion, name);
        checkHash(file, profile.sha256[name]);
        locales[name] = readJson(file);
        if (Object.values(locales[name]).some(value => typeof value !== "string")) throw new Error("번역 값 형식 오류");
    }
    const english = locales["en.json"];
    const keys = Object.keys(english);
    for (const locale of Object.values(locales)) {
        if (JSON.stringify(Object.keys(locale)) !== JSON.stringify(keys)) throw new Error("번역 키 또는 순서가 원문과 다릅니다.");
    }
    if (!["Aki_Data/Server/database/locales/global/en.json", "SPT_Data/Server/database/locales/global/en.json"].includes(profile.englishPath)) {
        throw new Error("원문 경로 형식 오류");
    }
    const installed = readJson(path.join(root, profile.englishPath));
    if (Object.keys(installed).length !== keys.length || keys.some(key => installed[key] !== english[key])) {
        throw new Error("설치된 서버 원문과 번역 기준이 다릅니다.");
    }
    return `[고라니 SPT 한글화 v2.2.0 | SPT ${version}] 한글화 파일 검증 및 적용 준비 완료! `
        + `(번역 기준 ${profile.translationVersion}, 한글판 ${Object.keys(locales["kr.json"]).length.toLocaleString("en-US")} / 한영 병기판 ${Object.keys(locales["kr-en.json"]).length.toLocaleString("en-US")}개)`;
}

class KoreanStatus {
    postDBLoad(container) {
        let logger;
        try { logger = container.resolve("PrimaryLogger"); }
        catch (_) { logger = container.resolve("WinstonLogger"); }
        try {
            // user/mods/<this mod>/src; independent of the process working directory.
            const root = path.resolve(__dirname, "../../../..");
            logger.success(verify(root));
            logger.info("번역은 게임 실행 시 클라이언트에 적용됩니다. 재밌는 SPT 되세요!");
        } catch (error) {
            logger.error(`[고라니 SPT 한글화 v2.2.0] 적용 준비 확인 실패: ${error.message}`);
        }
    }
}

module.exports = { mod: new KoreanStatus(), verify };
