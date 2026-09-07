"use strict";
const assert = require("node:assert/strict");
const fs = require("node:fs");
const os = require("node:os");
const path = require("node:path");
const { verify } = require("../src/ServerLocaleStatus3/src/mod.js");
const [stage, installations] = process.argv.slice(2);
const temporary = fs.mkdtempSync(path.join(os.tmpdir(), "spt-status-contract-"));
let checks = 0;
try {
    for (const version of ["3.8.3", "3.9.8", "3.10.5", "3.11.4"]) {
        const root = path.join(temporary, version);
        fs.cpSync(stage, root, { recursive: true });
        const game = path.join(installations, `SPT_${version}`);
        const data = version === "3.8.3" ? "Aki_Data" : "SPT_Data";
        for (const relative of ["EscapeFromTarkov.exe", `${data}/Server/configs/core.json`, `${data}/Server/database/locales/global/en.json`]) {
            const target = path.join(root, relative);
            fs.mkdirSync(path.dirname(target), { recursive: true });
            fs.copyFileSync(path.join(game, relative), target);
        }
        assert.match(verify(root), new RegExp(`v2.2.0 \\| SPT ${version.replaceAll(".", "\\.")}`));
        checks++;
        // Exercise the real server lifecycle export, including root discovery and logger resolution.
        const modPath = path.join(root, "user/mods/status/src/mod.js");
        fs.mkdirSync(path.dirname(modPath), { recursive: true });
        fs.copyFileSync(path.join(__dirname, "../src/ServerLocaleStatus3/src/mod.js"), modPath);
        const messages = [];
        const logger = { success: message => messages.push(message), info() {}, error: message => { throw new Error(message); } };
        require(modPath).mod.postDBLoad({ resolve: name => {
            if (version === "3.8.3" && name === "PrimaryLogger") throw new Error("legacy logger");
            return logger;
        } });
        assert.equal(messages.length, 1);
        console.log(messages[0]);
        checks++;
        for (const relative of ["BepInEx/plugins/GoLani.KoreanModFix.dll", `BepInEx/plugins/SPT-Korean/locales/${version}/kr.json`]) {
            const file = path.join(root, relative), original = fs.readFileSync(file);
            fs.appendFileSync(file, " ");
            assert.throws(() => verify(root), /파일 검증 실패/);
            fs.writeFileSync(file, original);
            checks++;
        }
        fs.writeFileSync(path.join(root, `${data}/Server/database/locales/global/en.json`), '{"wrong":"database"}');
        assert.throws(() => verify(root), /서버 원문/);
        checks++;
        fs.rmSync(path.join(root, "EscapeFromTarkov.exe"));
        assert.throws(() => verify(root), /게임 파일 설치/);
        checks++;
    }
    console.log(`Node server status contract passed: ${checks} cases.`);
} finally { fs.rmSync(temporary, { recursive: true, force: true }); }
