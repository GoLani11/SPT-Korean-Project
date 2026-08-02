# SPT-Korean-Project / SPT-한글화-프로젝트

## English

Hello! Welcome to SPT-Korean-Project.

My name is Golani, and I led this project. I am deeply grateful to Makina for working on the project with me. Also, thanks to Gomeng for actively providing this mod to me.

There are still mistranslations, so I would appreciate testing and reports.

### Supported SPT version

This repository currently targets SPT `4.1.x`, verified against SPT `4.1.0`.

### What this package contains

This repository now builds two runtime components:

```text
Server locale mod:      SPT_Runtime\user\mods\SPT_Korean_Localization
Client UI fix plugin:   BepInEx\plugins\GoLani.KoreanModFix.dll
```

The server mod applies Korean locale data through the SPT server. The client plugin applies Korean UI display fixes through BepInEx.

### How to apply it

For an SPT install at `D:\SPT`, run:

```powershell
pwsh -NoLogo -NoProfile -ExecutionPolicy Bypass -File .\tools\install-to-spt.ps1 -TargetSptRoot D:\SPT
```

The installed layout should include:

```text
D:\SPT\SPT_Runtime\user\mods\SPT_Korean_Localization\SPT_Korean_Localization.dll
D:\SPT\SPT_Runtime\user\mods\SPT_Korean_Localization\locale\kr.json
D:\SPT\BepInEx\plugins\GoLani.KoreanModFix.dll
```

To install only the server locale mod, use:

```powershell
pwsh -NoLogo -NoProfile -ExecutionPolicy Bypass -File .\tools\install-to-spt.ps1 -TargetSptRoot D:\SPT -SkipClientPlugin
```

### Manual package build

```powershell
pwsh -NoLogo -NoProfile -ExecutionPolicy Bypass -File .\tools\package-release.ps1 -SptRoot D:\SPT
```

The package is created under:

```text
artifacts\release\SPT_Runtime\user\mods\SPT_Korean_Localization
artifacts\release\BepInEx\plugins\GoLani.KoreanModFix.dll
```

Copy the `SPT_Runtime` and `BepInEx` folders from `artifacts\release` into the SPT install root if installing manually.

### GitHub release asset build

To create both GitHub release zip files, double-click:

```text
make-release-packages.bat
```

Or run:

```powershell
pwsh -NoLogo -NoProfile -ExecutionPolicy Bypass -File .\tools\package-release-variants.ps1 -SptRoot D:\SPT
```

The script creates:

```text
artifacts\release\SPT_Korean_Localization.KR.EN._G.M.zip
artifacts\release\SPT_Korean_Localization.KR._G.M.zip
```

The `KR.EN` zip keeps the bilingual locale. The `KR` zip is generated from the same build output and removes trailing English helper lines such as `\n(English item name)` from locale values.

### Source layout

```text
src\ServerLocaleMod
src\ClientModFixPlugin
```

The client plugin source was merged from:

https://github.com/GoLani11/GoLani.KoreanModFix

You can use the source code freely if you leave a comment and source address.

## 한국어

안녕하세요! SPT-한글화-프로젝트에 오신 것을 환영합니다.

저는 이 프로젝트를 주도하고 있는 고라니(Golani)입니다. 저와 함께 프로젝트에 처음부터 참여해주고 많은 도움을 주신 마키나(Makina)님께 깊은 감사의 말씀드립니다. 또한 이 모드를 적극적으로 제공해 주신 고맹(Gomeng)님께도 감사드립니다.

아직 오역이 많으니 테스트해 주시고 제보해 주시면 감사하겠습니다.

### 지원 SPT 버전

현재 이 저장소는 SPT `4.1.x`를 대상으로 하며, SPT `4.1.0`에서 확인했습니다.

### 포함된 구성

이 저장소는 이제 두 가지 구성물을 함께 빌드합니다.

```text
서버 로케일 모드:       SPT_Runtime\user\mods\SPT_Korean_Localization
클라이언트 UI 보정 플러그인: BepInEx\plugins\GoLani.KoreanModFix.dll
```

서버 모드는 SPT 서버에서 한국어 로케일을 적용합니다. 클라이언트 플러그인은 BepInEx를 통해 한국어 UI 표시 문제를 보정합니다.

### 모드 적용 방법

SPT가 `D:\SPT`에 설치되어 있다면 아래 명령을 실행하세요.

```powershell
pwsh -NoLogo -NoProfile -ExecutionPolicy Bypass -File .\tools\install-to-spt.ps1 -TargetSptRoot D:\SPT
```

설치 후 경로는 아래처럼 되어야 합니다.

```text
D:\SPT\SPT_Runtime\user\mods\SPT_Korean_Localization\SPT_Korean_Localization.dll
D:\SPT\SPT_Runtime\user\mods\SPT_Korean_Localization\locale\kr.json
D:\SPT\BepInEx\plugins\GoLani.KoreanModFix.dll
```

서버 로케일 모드만 설치하려면 아래처럼 실행하세요.

```powershell
pwsh -NoLogo -NoProfile -ExecutionPolicy Bypass -File .\tools\install-to-spt.ps1 -TargetSptRoot D:\SPT -SkipClientPlugin
```

### 수동 패키지 생성

```powershell
pwsh -NoLogo -NoProfile -ExecutionPolicy Bypass -File .\tools\package-release.ps1 -SptRoot D:\SPT
```

패키지는 아래 폴더에 생성됩니다.

```text
artifacts\release\SPT_Runtime\user\mods\SPT_Korean_Localization
artifacts\release\BepInEx\plugins\GoLani.KoreanModFix.dll
```

수동으로 설치할 때는 `artifacts\release` 안의 `SPT_Runtime` 폴더와 `BepInEx` 폴더를 SPT 설치 루트에 복사하세요.

### GitHub 릴리즈 파일 생성

GitHub 릴리즈에 올릴 zip 2종을 만들려면 아래 파일을 더블클릭하세요.

```text
make-release-packages.bat
```

명령으로 실행하려면 아래처럼 실행하세요.

```powershell
pwsh -NoLogo -NoProfile -ExecutionPolicy Bypass -File .\tools\package-release-variants.ps1 -SptRoot D:\SPT
```

생성되는 파일은 아래 2개입니다.

```text
artifacts\release\SPT_Korean_Localization.KR.EN._G.M.zip
artifacts\release\SPT_Korean_Localization.KR._G.M.zip
```

`KR.EN` zip은 한영 병기 로케일을 유지합니다. `KR` zip은 같은 빌드 결과에서 자동 생성하며, `\n(영어 아이템명)`처럼 값 끝에 붙은 영어 보조 줄을 제거합니다.

### 소스 구조

```text
src\ServerLocaleMod
src\ClientModFixPlugin
```

클라이언트 플러그인 소스는 아래 저장소에서 통합했습니다.

https://github.com/GoLani11/GoLani.KoreanModFix

댓글과 출처만 남겨주시면 자유롭게 소스 코드 활용하셔도 됩니다.

[![Hits](https://hits.seeyoufarm.com/api/count/incr/badge.svg?url=https%3A%2F%2Fgithub.com%2FGoLani11%2FSPT-Korean-Project-Alpha_Test&count_bg=%2346D3CF&title_bg=%23555555&icon=&icon_color=%23E7E7E7&title=%EB%B0%A9%EB%AC%B8%EC%9E%90+%EC%88%98&edge_flat=false)](https://hits.seeyoufarm.com)
