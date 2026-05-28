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
  "animationDurationMs": 72,
  "frameCount": 9,
  "minimumFrameDelta": 0,
  "wheelMultiplier": 2.8,
  "appProfiles": {
    "WINWORD": {
      "animationDurationMs": 35,
      "frameCount": 3,
      "minimumFrameDelta": 80,
      "wheelMultiplier": 7.0
    },
    "AcroRd32": {
      "animationDurationMs": 55,
      "frameCount": 7,
      "minimumFrameDelta": 15,
      "wheelMultiplier": 3.2
    },
    "explorer": {
      "animationDurationMs": 45,
      "frameCount": 5,
      "minimumFrameDelta": 25,
      "wheelMultiplier": 3.5
    }
  }
}
```

App se nhan dien process nam duoi con tro chuot. Neu ban chia man hinh thanh nhieu cua so, cuon tren cua so nao thi profile cua process do duoc ap dung.

Ten process thong dung:

- Word: `WINWORD`
- Adobe Reader: `AcroRd32`
- Adobe Acrobat: `Acrobat`
- Windows Explorer: `explorer`
- Edge: `msedge`
- Chrome: `chrome`

Goi y:

- Muon muot hon: dung menu tray `Use smoother preset`.
- Muon nhanh hon: dung menu tray `Use faster preset`.
- Muon phan hoi nhanh hon nua: giam `animationDurationMs` xuong 45-60.
- Muon cuon nhanh hon: tang `wheelMultiplier` len 3.0-4.0.
- Rieng Word thuong can `wheelMultiplier` cao hon va `minimumFrameDelta` lon hon vi Word xu ly wheel theo nac dong thay vi pixel.
- Muon bot giat trong app co ho tro high-resolution wheel: giam `minimumFrameDelta` xuong 10-20.
- Muon tranh lag trong Word/PDF/Explorer: giu `minimumFrameDelta` khoang 30-60.

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
