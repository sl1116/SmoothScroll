# Kien truc MVP

Tai lieu nay giai thich ban dau app dang lam gi, theo ngon ngu de doc nhat co the.

## Muc tieu

App chay nen tren Windows va lam cho thao tac cuon chuot bot giat. Khi ban cuon mot nac chuot, Windows thuong gui mot tin hieu lon, vi du `120`. App nay chan tin hieu do lai, sau do gui lai nhieu tin hieu nho hon trong khoang thoi gian ngan.

Vi du:

```text
Wheel goc: 120
App phat lai: 33, 26, 20, 15, 11, 7, 5, 3...
```

Nhan vao mat nguoi dung, noi dung tren man hinh co cam giac di chuyen mem hon.

## Cac phan chinh

### `Program.cs`

Diem bat dau cua app. File nay:

- Bat WinForms.
- Load cau hinh.
- Tao animation engine.
- Cai mouse hook.
- Chay system tray icon.

### `MouseHook.cs`

Phan nay bat su kien mouse wheel bang Windows low-level hook.

Neu app dang bat va app hien tai khong nam trong danh sach disable:

1. Lay delta cua wheel.
2. Dua delta vao animation engine.
3. Tra ve `1` de chan wheel goc.

Neu wheel la su kien do chinh app tao ra, no se di qua binh thuong de tranh vong lap vo han.

### `SmoothScrollAnimator.cs`

Phan nay bien mot wheel delta lon thanh nhieu frame nho.

Cac setting quan trong:

- `AnimationDurationMs`: animation keo dai bao lau.
- `FrameCount`: chia thanh bao nhieu buoc.
- `WheelMultiplier`: tang/giam do manh cua scroll.

### `TrayAppContext.cs`

Tao icon trong System Tray va menu chuot phai.

Menu hien tai co:

- Enable SmoothScroll
- Disable for current app
- Open config file
- Exit

### `SettingsStore.cs`

Doc/ghi file cau hinh:

```text
%AppData%\SmoothScrollLocal\config.json
```

Ban co the sua file nay khi app dang chay. App se tu reload cau hinh.

## Gioi han hien tai

Day moi la prototype, nen co vai gioi han quan trong:

- Mot so app khong xu ly wheel delta nho tot, nen co the chua muot nhu mong muon.
- Chua co UI Settings dep.
- Chua co installer.
- Chua co auto-start cung Windows.
- Chua co profile rieng cho Word/PDF/Explorer.

## Buoc tiep theo nen lam

1. Chay thu tren Word, PDF reader, Explorer.
2. Ghi lai app nao muot, app nao khong.
3. Them UI Settings don gian.
4. Them profile theo app.
5. Dong goi thanh file cai dat.
6. Nghien cuu Linux X11 sau khi Windows MVP on dinh.

