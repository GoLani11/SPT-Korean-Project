import { IPostDBLoadMod } from "@spt/models/external/IPostDBLoadMod";
import { ILogger } from "@spt/models/spt/utils/ILogger";
import { DatabaseServer } from "@spt/servers/DatabaseServer";
import { DependencyContainer } from "tsyringe";
import * as path from 'path';

class KoreanPatcher implements IPostDBLoadMod {
    private koreanPatch: { [key: string]: string };
    private readonly patchPath = path.join(__dirname, '..', 'locale', 'kr.json');

    constructor() {
        this.koreanPatch = require(this.patchPath);
    }

    public postDBLoad(container: DependencyContainer): void {
        const logger = container.resolve<ILogger>("WinstonLogger");
        const databaseServer = container.resolve<DatabaseServer>("DatabaseServer");
        const tables = databaseServer.getTables();
        
        if (!tables.locales.global["kr"]) {
            logger.error("기존 한국어 언어파일을 찾을 수 없습니다. .../SPT_Data/Server/database/locales/global/kr.json 이 있는지 확인하세요.");
            return;
        }

        const originalkr = tables.locales.global["kr"];
        const startTime = Date.now();

        try {
            // Object.assign을 사용하여 한 번에 모든 속성을 복사
            Object.assign(originalkr, this.koreanPatch);

            const endTime = Date.now();
            const updateCount = Object.keys(this.koreanPatch).length;
            
            logger.info(`한국어 패치가 성공적으로 적용되었습니다. 적용된 항목 줄 수: ${updateCount}`);
        } catch (error) {
            logger.error(`한국어 패치 적용 중 오류 발생: ${error.message}`);
        }
    }
}

module.exports = { mod: new KoreanPatcher() }