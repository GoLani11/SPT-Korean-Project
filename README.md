# SPT Korean Localization / SPT 한글화 프로젝트

**현재 배포 버전 / Current release: 2.1.0**

[한국어](#한국어) · [English](#english) · [Releases](../../releases) · [Issues](../../issues)

---

## 한국어

### 프로젝트 소개

SPT 서버 한글 번역과 한국어 UI 표시 보정을 하나의 패키지로 제공합니다. Golani와 Makina가 제작했으며, Gomeng이 배포를 도왔습니다.

번역은 지속적으로 보완 중입니다. 오역이나 미번역을 발견하면 사용 중인 SPT 버전, 표시된 문구와 위치를 [Issues](../../issues)에 남겨 주세요.

### 2.1.0 주요 변경 사항

- 한글판과 한영 병기판을 버전별 ZIP 하나로 통합
- 게임의 `설정 > 게임 > 인터페이스 언어`에서 `한국어` 바로 아래에 `한국어 (Korean)` 추가
- 게임이나 서버를 재시작하지 않고 한글판과 한영 병기판 전환
- 총 7개 ZIP에 포함된 두 로케일의 키 순서, 내부 경로, 대상 DLL과 SHA-256 자동 검증

### 다운로드 파일 선택

SPT 3.x, 4.0.13, 4.1.0은 설치 버전과 **정확히 같은 ZIP 하나**를 받아 주세요. SPT 4.1.2와 4.1.3은 두 버전이 함께 표시된 공용 ZIP을 사용하면 됩니다. SPT 4.1.1은 지원하지 않습니다.

| SPT 버전 | 다운로드 파일 |
| --- | --- |
| 3.8.3 | `SPT-KR-3.8.3.zip` |
| 3.9.8 | `SPT-KR-3.9.8.zip` |
| 3.10.5 | `SPT-KR-3.10.5.zip` |
| 3.11.4 | `SPT-KR-3.11.4.zip` |
| 4.0.13 | `SPT-KR-4.0.13.zip` |
| 4.1.0 | `SPT-KR-4.1.0.zip` |
| 4.1.2–4.1.3 | `SPT-KR-4.1.2-4.1.3.zip` |

각 ZIP에는 두 표시 방식이 모두 들어 있습니다.

- `한국어`: 기본 한글판입니다. 기존 형식에 따라 퀘스트 제목·목표, 아이템 설명의 영문 머리말과 일부 레이드 탈출구에는 영어가 함께 표시될 수 있습니다.
- `한국어 (Korean)`: 전체 한영 병기판입니다.

다른 SPT 버전용 ZIP이나 다른 한국어 로케일 모드를 함께 설치하면 충돌할 수 있습니다.

### 설치 방법

1. 실행 중인 SPT 서버와 게임을 모두 종료하세요.
2. 선택한 ZIP을 **SPT 설치 최상위 폴더**에 바로 압축 해제하세요.
3. 기존 파일을 바꿀지 물으면 덮어쓰세요.
4. 서버를 실행한 뒤 `설정 > 게임 > 인터페이스 언어`에서 `한국어` 또는 `한국어 (Korean)`를 선택하세요.

두 표시 방식은 게임과 서버를 재시작하지 않고 전환할 수 있으며, 선택값은 게임 설정에 저장됩니다.

ZIP에는 선택한 SPT 버전에 필요한 파일만 들어 있으며, 설치용 BAT나 EXE는 포함하지 않습니다. 기존 `spt_korean_localization_G&M` 또는 `SPT_Korean_Localization` 정식 버전은 같은 경로에 덮어쓸 수 있습니다.

아주 오래된 alpha 버전을 사용했다면 설치 전에 아래 폴더를 직접 삭제하세요.

```text
user\mods\spt_korean_localization_alpha_test_gm
```

### 버전별 설치 결과

```text
SPT 3.8.3–3.11.4
├─ BepInEx\plugins\GoLani.KoreanModFix.dll
└─ user\mods\spt_korean_localization_G&M

SPT 4.0.13
├─ BepInEx\plugins\GoLani.KoreanModFix.dll
└─ SPT\user\mods\SPT_Korean_Localization

SPT 4.1.0 / 4.1.2–4.1.3
├─ BepInEx\plugins\GoLani.KoreanModFix.dll
└─ SPT_Runtime\user\mods\SPT_Korean_Localization
```

SPT 4.1.0 ZIP에는 해당 버전 전용 서버 모드와 클라이언트 플러그인이 들어 있습니다. 4.1.2–4.1.3 ZIP은 두 버전에서 공용으로 검증한 바이너리와 번역을 사용합니다. 서로 바꿔 설치하거나 표시되지 않은 SPT 버전에 설치하지 마세요.

### 개발용 릴리스 빌드

빌드하려면 .NET 10 SDK, SPT 클라이언트 참조 파일과 같은 상위 폴더의 `spt-korean-translate` 저장소가 필요합니다. `make-release-packages.bat`은 `D:\SPT3.8.3`을 먼저 확인하고, 없으면 `D:\SPT`를 사용합니다.

```powershell
..\spt-korean-translate\.venv\Scripts\python.exe .\tools\package_release_versions.py
```

또는 `make-release-packages.bat`을 실행하면 됩니다. 생성된 ZIP 7개와 검증 요약은 `artifacts\release`에 저장되며 Git에는 포함되지 않습니다.

### 라이선스

소스 코드는 [MIT License](LICENSE.md)를 따릅니다. 클라이언트 플러그인 코드를 재사용할 때는 프로젝트 이름과 저장소 주소를 남겨 주세요.

---

## English

### About

This project provides SPT server-side Korean localization and Korean UI display fixes in one package. It is created by Golani and Makina, with distribution support from Gomeng.

The translation is continuously maintained. If you find a mistranslation or untranslated text, please report the SPT version, the displayed text, and where it appears in [Issues](../../issues).

### What's new in 2.1.0

- Combined the Korean and full Korean–English editions into one ZIP per supported version.
- Added `한국어 (Korean)` immediately below `Korean` in the existing in-game interface-language setting.
- Made both display modes switchable without restarting the game or server.
- Strengthened the release pipeline to validate both locale payloads, archive paths, target DLLs, and SHA-256 hashes across all seven ZIP files.

### Choose a download

For SPT 3.x, 4.0.13, and 4.1.0, download the **one ZIP that exactly matches your installed version**. SPT 4.1.2 and 4.1.3 use the shared ZIP labelled with both versions. SPT 4.1.1 is not supported.

| SPT version | Download |
| --- | --- |
| 3.8.3 | `SPT-KR-3.8.3.zip` |
| 3.9.8 | `SPT-KR-3.9.8.zip` |
| 3.10.5 | `SPT-KR-3.10.5.zip` |
| 3.11.4 | `SPT-KR-3.11.4.zip` |
| 4.0.13 | `SPT-KR-4.0.13.zip` |
| 4.1.0 | `SPT-KR-4.1.0.zip` |
| 4.1.2–4.1.3 | `SPT-KR-4.1.2-4.1.3.zip` |

Each ZIP contains both display modes.

- `Korean`: Primarily Korean. To preserve the established format, English may still appear alongside quest titles and objectives, as item-description headers, and on some raid extraction names.
- `한국어 (Korean)`: Full Korean–English bilingual edition.

Do not install a ZIP for another SPT version or use it alongside another Korean locale mod.

### Installation

1. Close the running SPT server and game.
2. Extract the selected ZIP directly into the **top-level SPT installation folder**.
3. Allow existing files to be overwritten when prompted.
4. Start the server, then choose `Korean` or `한국어 (Korean)` under `Settings > Game > Interface Language`.

You can switch between both modes without restarting the game or server. The game saves the selected language normally.

Each ZIP contains only the files required for its target SPT version and contains no BAT or EXE installer. An existing official `spt_korean_localization_G&M` or `SPT_Korean_Localization` installation can be overwritten in place.

If you used a very old alpha build, delete this folder before installation:

```text
user\mods\spt_korean_localization_alpha_test_gm
```

### Installed layout by version

```text
SPT 3.8.3–3.11.4
├─ BepInEx\plugins\GoLani.KoreanModFix.dll
└─ user\mods\spt_korean_localization_G&M

SPT 4.0.13
├─ BepInEx\plugins\GoLani.KoreanModFix.dll
└─ SPT\user\mods\SPT_Korean_Localization

SPT 4.1.0 / 4.1.2–4.1.3
├─ BepInEx\plugins\GoLani.KoreanModFix.dll
└─ SPT_Runtime\user\mods\SPT_Korean_Localization
```

The SPT 4.1.0 ZIP contains server and client binaries gated specifically to 4.1.0. The 4.1.2–4.1.3 ZIP uses binaries and translations validated for both versions. Do not swap these packages or install them on an unlisted SPT version.

### Development release build

Building requires the .NET 10 SDK, SPT client reference files, and the sibling `spt-korean-translate` repository. `make-release-packages.bat` checks `D:\SPT3.8.3` first and falls back to `D:\SPT`.

```powershell
..\spt-korean-translate\.venv\Scripts\python.exe .\tools\package_release_versions.py
```

Alternatively, run `make-release-packages.bat`. The seven generated ZIP files and their validation summary are written to `artifacts\release` and are not tracked by Git.

### License

The source code is available under the [MIT License](LICENSE.md). If you reuse the client plugin code, retain the project name and repository URL.
