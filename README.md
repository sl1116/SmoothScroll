# SmoothScroll Local

Prototype app lam muot thao tac cuon chuot tren Windows.

Ban MVP nay dung C#/.NET vi may hien tai da co san .NET SDK, giup chay thu nhanh truoc khi can dau tu vao Rust/C++.

Trang goc de tham khao hanh vi: https://www.smoothscroll.net/win/

## Trang thai hien tai

Day la ban MVP dau tien:

- Chay nen trong Windows System Tray.
- Bat su kien mouse wheel o cap he dieu hanh bang WinAPI low-level hook.
- Chan wheel goc va phat lai thanh nhieu buoc nho de tao animation.
- Co menu bat/tat nhanh.
- Co chuc nang disable cho app dang active.
- Luu cau hinh trong `%AppData%\SmoothScrollLocal\config.json`.

Luu y: day chua phai ban san pham hoan chinh. Mot so app chi xu ly wheel theo nac 120 don vi, nen animation co the muot hon o app nay nhung kem hon o app khac. Day la diem can nghien cuu tiep cho Word/PDF/Explorer.

## Yeu cau

- Windows 10/11.
- .NET 8 SDK.

Kiem tra:

```powershell
dotnet --version
```

## Chay thu

```powershell
dotnet run
```

Sau khi chay, tim icon cua app trong System Tray. Chuot phai vao icon de:

- Enable SmoothScroll
- Disable for current app
- Open config file
- Exit

## Chinh toc do

App luu cau hinh tai:

```text
%AppData%\SmoothScrollLocal\config.json
```

Gia tri mac dinh hien tai:

```json
{
  "animationDurationMs": 95,
  "frameCount": 8,
  "wheelMultiplier": 1.35
}
```

Goi y:

- Muon phan hoi nhanh hon: giam `animationDurationMs` xuong 70-85.
- Muon cuon nhanh hon: tang `wheelMultiplier` len 1.5-1.8.
- Muon muot hon nhung co the cham hon: tang `frameCount` len 10-12.

## Build

```powershell
dotnet build -c Release
```

File build nam trong:

```text
bin\Release\net8.0-windows\
```

## Roadmap

1. Windows MVP
   - Bat/tat smooth scroll.
   - Cau hinh duration, frame count, wheel multiplier.
   - Disable theo app.

2. Windows nang cao
   - UI Settings day du.
   - Auto-start cung Windows.
   - Installer.
   - Profile rieng cho Word, PDF reader, Explorer.
   - Cach scroll tot hon cho Win32 controls cu.

3. Ubuntu/Linux
   - X11 truoc vi de can thiep input hon.
   - Wayland sau, voi gioi han bao mat ro rang hon.

## Giai thich them

Doc [docs/architecture.md](docs/architecture.md) de hieu cac file dang lam gi.
