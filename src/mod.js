"use strict";
var __createBinding = (this && this.__createBinding) || (Object.create ? (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    var desc = Object.getOwnPropertyDescriptor(m, k);
    if (!desc || ("get" in desc ? !m.__esModule : desc.writable || desc.configurable)) {
      desc = { enumerable: true, get: function() { return m[k]; } };
    }
    Object.defineProperty(o, k2, desc);
}) : (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    o[k2] = m[k];
}));
var __setModuleDefault = (this && this.__setModuleDefault) || (Object.create ? (function(o, v) {
    Object.defineProperty(o, "default", { enumerable: true, value: v });
}) : function(o, v) {
    o["default"] = v;
});
var __importStar = (this && this.__importStar) || function (mod) {
    if (mod && mod.__esModule) return mod;
    var result = {};
    if (mod != null) for (var k in mod) if (k !== "default" && Object.prototype.hasOwnProperty.call(mod, k)) __createBinding(result, mod, k);
    __setModuleDefault(result, mod);
    return result;
};
Object.defineProperty(exports, "__esModule", { value: true });
const path = __importStar(require("path"));
class KoreanPatcher {
    koreanPatch;
    patchPath = path.join(__dirname, '..', 'locale', 'kr.json');
    constructor() {
        this.koreanPatch = require(this.patchPath);
    }
    postDBLoad(container) {
        const logger = container.resolve("WinstonLogger");
        const databaseServer = container.resolve("DatabaseServer");
        const tables = databaseServer.getTables();
        if (!tables.locales.global["kr"]) {
            logger.error("기존 한국어 언어파일을 찾을 수 없습니다.");
            return;
        }
        const originalkr = tables.locales.global["kr"];
        const startTime = Date.now();
        try {
            // Object.assign을 사용하여 한 번에 모든 속성을 복사
            Object.assign(originalkr, this.koreanPatch);
            const endTime = Date.now();
            const updateCount = Object.keys(this.koreanPatch).length;
            logger.info(`한국어 패치가 성공적으로 적용되었습니다. 업데이트된 항목: ${updateCount}`);
        }
        catch (error) {
            logger.error(`한국어 패치 적용 중 오류 발생: ${error.message}`);
        }
    }
}
module.exports = { mod: new KoreanPatcher() };
//# sourceMappingURL=mod.js.map