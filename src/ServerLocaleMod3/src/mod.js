"use strict";

const fs = require("fs");
const path = require("path");

class KoreanPatcher {
    constructor() {
        this.patchPath = path.join(__dirname, "..", "locale", "kr.json");
        this.koreanPatch = require(this.patchPath);
        const manifest = require(path.join(__dirname, "..", "package.json"));
        this.expectedVersion = manifest.akiVersion ?? manifest.sptVersion;
    }

    postDBLoad(container) {
        const logger = this.resolveLogger(container);

        try {
            const installedVersion = this.detectInstalledVersion();
            if (installedVersion !== this.expectedVersion) {
                logger.error(
                    `SPT 한글화 ${this.expectedVersion} 패키지는 현재 SPT ${installedVersion ?? "알 수 없음"}에서 비활성화됩니다. ` +
                    "설치된 SPT와 정확히 같은 버전의 ZIP을 사용하세요."
                );
                return;
            }

            const databaseServer = container.resolve("DatabaseServer");
            const tables = databaseServer.getTables();
            const koreanLocale = tables?.locales?.global?.kr;

            if (!koreanLocale) {
                logger.error("기존 한국어 언어파일을 찾을 수 없습니다. Aki_Data 또는 SPT_Data의 한국어 로케일을 확인하세요.");
                return;
            }

            const startTime = Date.now();
            Object.assign(koreanLocale, this.koreanPatch);
            const elapsed = Date.now() - startTime;
            const updateCount = Object.keys(this.koreanPatch).length;

            logger.info("고라니 SPT 한글화 프로젝트가 정상적으로 적용되었습니다. 재밌는 SPT되세요!");
            logger.info(`적용된 항목 줄 수: ${updateCount} (처리 시간: ${elapsed}ms)`);
        }
        catch (error) {
            logger.error(`고라니 SPT 한글화 프로젝트 적용 중 오류 발생: ${error?.stack ?? error}`);
        }
    }

    detectInstalledVersion() {
        const candidates = [
            path.join(process.cwd(), "Aki_Data", "Server", "configs", "core.json"),
            path.join(process.cwd(), "SPT_Data", "Server", "configs", "core.json")
        ];

        for (const candidate of candidates) {
            if (!fs.existsSync(candidate)) {
                continue;
            }

            const coreConfig = JSON.parse(fs.readFileSync(candidate, "utf8"));
            return coreConfig.akiVersion ?? coreConfig.sptVersion ?? null;
        }

        return null;
    }

    resolveLogger(container) {
        for (const token of ["PrimaryLogger", "WinstonLogger"]) {
            try {
                return container.resolve(token);
            }
            catch (_) {
                // Try the logger token used by the other supported 3.x family.
            }
        }

        return console;
    }
}

module.exports = { mod: new KoreanPatcher() };
