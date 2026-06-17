# SPT-Korean-Project / SPT-한글화-프로젝트

## English

Hello! Welcome to SPT-Korean-Project.

My name is Golani, and I led this project. I am deeply grateful to Makina for working on the project with me. Also, thanks to Gomeng for actively providing this mod to me.

There are still mistranslations, so I would appreciate testing and reports.

### Supported SPT version

This repository currently targets SPT `4.0.x` as a C# server mod.

### How to apply it

For an SPT install at `D:\SPT`, run:

```powershell
pwsh -NoLogo -NoProfile -ExecutionPolicy Bypass -File .\tools\install-to-spt.ps1 -TargetSptRoot D:\SPT
```

The installed layout should be:

```text
D:\SPT\SPT\user\mods\SPT_Korean_Localization\SPT_Korean_Localization.dll
D:\SPT\SPT\user\mods\SPT_Korean_Localization\locale\kr.json
```

This repository's current output is a server mod. Do not install `SPT_Korean_Localization.dll` into `D:\SPT\BepInEx\plugins`.

### Manual package build

```powershell
pwsh -NoLogo -NoProfile -ExecutionPolicy Bypass -File .\tools\package-release.ps1
```

The package is created under:

```text
release\SPT\user\mods\SPT_Korean_Localization
```

Copy the `SPT` folder from `release` into the SPT install root if installing manually.

### Plugin source code credit

Earlier plugin source reference:

https://github.com/GoLani11/GoLani.KoreanModFix

You can use the source code freely if you leave a comment and source address.

## 한국어

안녕하세요! SPT-한글화-프로젝트에 오신 것을 환영합니다.

저는 이 프로젝트를 주도하고 있는 고라니(Golani)입니다. 저와 함께 프로젝트에 처음부터 참여해주고 많은 도움을 주신 마키나(Makina)님께 깊은 감사의 말씀드립니다. 또한 이 모드를 적극적으로 제공해 주신 고맹(Gomeng)님께도 감사드립니다.

아직 오역이 많으니 테스트해 주시고 제보해 주시면 감사하겠습니다.

### 지원 SPT 버전

현재 이 저장소는 SPT `4.0.x`용 C# 서버 모드입니다.

### 모드 적용 방법

SPT가 `D:\SPT`에 설치되어 있다면 아래 명령을 실행하세요.

```powershell
pwsh -NoLogo -NoProfile -ExecutionPolicy Bypass -File .\tools\install-to-spt.ps1 -TargetSptRoot D:\SPT
```

설치 후 경로는 아래처럼 되어야 합니다.

```text
D:\SPT\SPT\user\mods\SPT_Korean_Localization\SPT_Korean_Localization.dll
D:\SPT\SPT\user\mods\SPT_Korean_Localization\locale\kr.json
```

현재 이 저장소에서 빌드되는 파일은 서버 모드입니다. `SPT_Korean_Localization.dll`을 `D:\SPT\BepInEx\plugins`에 넣지 마세요.

### 수동 패키지 생성

```powershell
pwsh -NoLogo -NoProfile -ExecutionPolicy Bypass -File .\tools\package-release.ps1
```

패키지는 아래 폴더에 생성됩니다.

```text
release\SPT\user\mods\SPT_Korean_Localization
```

수동으로 설치할 때는 `release` 안의 `SPT` 폴더를 SPT 설치 루트에 복사하세요.

### 플러그인 소스 코드 출처

이전 플러그인 소스 참고:

https://github.com/GoLani11/GoLani.KoreanModFix

댓글과 출처만 남겨주시면 자유롭게 소스 코드 활용하셔도 됩니다.

[![Hits](https://hits.seeyoufarm.com/api/count/incr/badge.svg?url=https%3A%2F%2Fgithub.com%2FGoLani11%2FSPT-Korean-Project-Alpha_Test&count_bg=%2346D3CF&title_bg=%23555555&icon=&icon_color=%23E7E7E7&title=%EB%B0%A9%EB%AC%B8%EC%9E%90+%EC%88%98&edge_flat=false)](https://hits.seeyoufarm.com)
