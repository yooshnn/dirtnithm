<img src="Dirtnithm.App/Resources/app.png" width="120"/>

# Dirtnithm

웹캠으로 양손 높이를 인식해서 키 입력을 제어하는 Windows 앱.

## 요구사항

- Windows 10/11
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Python 3.12
- [Interception 드라이버](https://github.com/oblitum/Interception/releases)


## 셋업

```powershell
git clone https://github.com/yooshnn/dirtnithm.git
cd dirtnithm/dirtnithm_vision
./setup.bat
cd ..
```

## 빌드

```powershell
cd dirtnithm_vision
.venv/Scripts/pyinstaller.exe build.spec --noconfirm
cd ..
dotnet publish Dirtnithm.App/Dirtnithm.App.csproj -c Release -o build
```

## 실행

```powershell
cd build
./Dirtnithm.App.exe
```
