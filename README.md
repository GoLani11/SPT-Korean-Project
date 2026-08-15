# SPT Korean Localization / SPT 한글화 프로젝트

SPT 서버 한글 번역과 한국어 UI 표시 보정을 함께 제공해요. Golani와 Makina가 제작하고 Gomeng이 배포에 도움을 주셨어요.

## 다운로드 파일 선택

설치된 SPT와 **버전이 정확히 같은 ZIP 하나**를 받으세요. `KR`은 한글판,
`KR-EN`은 전체 한영 병기판이에요. `KR`도 기존 사용 방식에 맞춰 퀘스트
제목·목표, 아이템 설명 영문 머리말과 레이드 탈출구 병기는 유지해요.

| SPT | 한글판 | 한영 병기판 |
| --- | --- | --- |
| 3.8.3 | `SPT_Korean_Localization.SPT-3.8.3.KR.GM.zip` | `SPT_Korean_Localization.SPT-3.8.3.KR-EN.GM.zip` |
| 3.9.8 | `SPT_Korean_Localization.SPT-3.9.8.KR.GM.zip` | `SPT_Korean_Localization.SPT-3.9.8.KR-EN.GM.zip` |
| 3.10.5 | `SPT_Korean_Localization.SPT-3.10.5.KR.GM.zip` | `SPT_Korean_Localization.SPT-3.10.5.KR-EN.GM.zip` |
| 3.11.4 | `SPT_Korean_Localization.SPT-3.11.4.KR.GM.zip` | `SPT_Korean_Localization.SPT-3.11.4.KR-EN.GM.zip` |
| 4.0.13 | `SPT_Korean_Localization.SPT-4.0.13.KR.GM.zip` | `SPT_Korean_Localization.SPT-4.0.13.KR-EN.GM.zip` |
| 4.1.0 | `SPT_Korean_Localization.SPT-4.1.0.KR.GM.zip` | `SPT_Korean_Localization.SPT-4.1.0.KR-EN.GM.zip` |

## 설치

1. 실행 중인 SPT 서버와 게임을 종료하세요.
2. 선택한 ZIP을 **SPT 설치 최상위 폴더**에 바로 압축 해제하세요.
3. 기존 파일을 바꿀지 물으면 덮어쓰세요.
4. 서버를 실행하고 게임 언어를 한국어로 선택하세요.

ZIP에는 해당 SPT 버전의 파일만 들어 있어 다른 버전용 폴더가 남지 않아요. 설치용 BAT나 EXE도 포함하지 않아요.

기존 `spt_korean_localization_G&M` 또는 `SPT_Korean_Localization` 정식 버전은 같은 경로에 덮어쓸 수 있어요. 아주 오래된 alpha 버전을 사용했다면 설치 전에 아래 폴더를 직접 삭제하세요.

```text
user\mods\spt_korean_localization_alpha_test_gm
```

## 버전별 설치 결과

```text
SPT 3.8.3–3.11.4
├─ BepInEx\plugins\GoLani.KoreanModFix.dll
└─ user\mods\spt_korean_localization_G&M

SPT 4.0.13
├─ BepInEx\plugins\GoLani.KoreanModFix.dll
└─ SPT\user\mods\SPT_Korean_Localization

SPT 4.1.0
├─ BepInEx\plugins\GoLani.KoreanModFix.dll
└─ SPT_Runtime\user\mods\SPT_Korean_Localization
```

다른 SPT 버전에는 설치하지 마세요. 서버 모드는 정확한 버전만 허용하고, 클라이언트 플러그인은 미지원 버전에서 패치를 비활성화해요.

## 개발용 릴리스 빌드

빌드에는 .NET 10 SDK, `D:\SPT3.8.3` 클라이언트 참조 파일, 그리고 같은 상위 폴더의 `spt-korean-translate` 저장소가 필요해요.

```powershell
..\spt-korean-translate\.venv\Scripts\python.exe .\tools\package_release_versions.py
```

또는 `make-release-packages.bat`을 실행하면 돼요. 생성된 12개 ZIP과 검증 요약은 `artifacts\release`에 저장되며 Git에는 포함되지 않아요.

## License

소스 코드는 [MIT License](LICENSE.md)를 따라요. 클라이언트 플러그인 코드를 재사용할 때는 프로젝트 이름과 저장소 주소를 남겨 주세요.
