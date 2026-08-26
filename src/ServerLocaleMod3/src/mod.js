"use strict";

const fs = require("fs");
const path = require("path");

class KoreanPatcher {
    constructor() {
        this.koreanPatch = require(path.join(__dirname, "..", "locale", "kr.json"));
        this.bilingualPatch = require(path.join(__dirname, "..", "locale", "kr-en.json"));
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
            const locales = tables?.locales;
            const koreanLocale = locales?.global?.kr;

            if (!koreanLocale) {
                logger.error("기존 한국어 언어파일을 찾을 수 없습니다. Aki_Data 또는 SPT_Data의 한국어 로케일을 확인하세요.");
                return;
            }
            if (!locales?.menu?.kr) {
                logger.error("기존 한국어 메뉴 언어파일을 찾을 수 없습니다.");
                return;
            }

            const startTime = Date.now();
            Object.assign(koreanLocale, this.koreanPatch);
            koreanLocale["kr-en"] = "한국어 (Korean)";

            for (const locale of Object.values(locales.global)) {
                locale["kr-en"] = "한국어 (Korean)";
            }

            locales.global["kr-en"] = {
                ...koreanLocale,
                ...this.bilingualPatch,
                "kr-en": "한국어 (Korean)"
            };
            locales.menu["kr-en"] = { ...locales.menu.kr };
            this.insertBilingualLanguageAfterKorean(locales.languages);

            const elapsed = Date.now() - startTime;
            const koreanCount = Object.keys(this.koreanPatch).length;
            const bilingualCount = Object.keys(this.bilingualPatch).length;

            logger.info("고라니 SPT 한글화 프로젝트가 정상적으로 적용되었습니다. 재밌는 SPT되세요!");
            logger.info(`적용된 항목 줄 수: 한글판 ${koreanCount}, 한영 병기판 ${bilingualCount} (처리 시간: ${elapsed}ms)`);
        }
        catch (error) {
            logger.error(`고라니 SPT 한글화 프로젝트 적용 중 오류 발생: ${error?.stack ?? error}`);
        }
    }

    insertBilingualLanguageAfterKorean(languages) {
        const existingLanguages = Object.entries(languages)
            .filter(([localeId]) => localeId.toLowerCase() !== "kr-en");

        for (const localeId of Object.keys(languages)) {
            delete languages[localeId];
        }

        let inserted = false;
        for (const [localeId, localeName] of existingLanguages) {
            languages[localeId] = localeName;
            if (localeId.toLowerCase() === "kr") {
                languages["kr-en"] = "Korean-English";
                inserted = true;
            }
        }

        if (!inserted) {
            languages["kr-en"] = "Korean-English";
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
