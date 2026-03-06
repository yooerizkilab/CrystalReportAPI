# CrystalReportAPI

ASP.NET Web API (.NET Framework 4.7.2) untuk membuat dan menampilkan daftar Crystal Reports. API ini terhubung ke SAP HANA (atau database lain melalui DSN) dan mengembalikan laporan dalam format string Base64.

## Fitur Utama

- **Pembuatan Laporan**: Konversi file `.rpt` langsung menjadi string PDF Base64.
- **Daftar Template**: Menampilkan daftar file laporan yang tersedia secara dinamis dari folder tertentu.
- **Konfigurasi Aman**: Kredensial sensitif (DSN, User, Password) disimpan di file eksternal yang dipisahkan dan diabaikan oleh Git.

## Prasyarat

- **Visual Studio 2022** atau versi terbaru.
- **.NET Framework 4.7.2**.
- **Crystal Reports Runtime** (Versi 13.0.4000.0 atau yang kompatibel).
- **SAP HANA Client / ODBC Driver** (jika menggunakan SAP HANA).

## Instalasi

1.  Clone repositori ini:
    ```bash
    git clone https://github.com/username-anda/CrystalReportAPI.git
    cd CrystalReportAPI
    ```
2.  Buka file `CrystalReportAPI.slnx` (atau `.csproj`) menggunakan Visual Studio.
3.  Restore paket NuGet:
    ```bash
    nuget restore
    ```

## Konfigurasi Keamanan

Demi keamanan, kredensial database **tidak** disimpan di `Web.config`. Anda harus membuat file bernama `web.secrets.config` di dalam folder proyek `CrystalReportAPI`.

### Membuat `web.secrets.config`

Buat file di `CrystalReportAPI/web.secrets.config` dengan format berikut:

```xml
<appSettings>
  <add key="DSN_NAME" value="NAMA_DSN_ANDA" />
  <add key="DB_USER" value="USER_DB_ANDA" />
  <add key="DB_PASSWORD" value="PASSWORD_DB_ANDA" />
</appSettings>
```

> [!IMPORTANT]
> File ini sudah terdaftar di `.gitignore` agar tidak sengaja ter-push ke repositori publik.

## Cara Penggunaan (API)

### 1. Generate Report (Base64)

Mengambil string Base64 dari laporan yang digenerate.

- **URL**: `GET /api/report/{schema}/{folder}/{rptFile}/{docEntry}`
- **Contoh**: `GET /api/report/SBO_LIVE/INVOICE/INV_A1/12345`

### 2. Daftar Template

Menampilkan semua file `.rpt` dalam sub-folder tertentu di dalam direktori `Layouts`.

- **URL**: `GET /api/template/{folderName}`
- **Contoh**: `GET /api/template/SO` (Akan list file di `Layouts/SO`)

### 3. Halaman Bantuan (Help Page)

Proyek ini dilengkapi dengan halaman dokumentasi API otomatis:

- **URL**: `GET /Help`

## Struktur Direktori

- `Controllers/`: Logika endpoint API.
- `Services/`: Logika bisnis untuk pemrosesan Crystal Reports.
- `Helpers/`: Utilitas untuk koneksi database dan konfigurasi.
- `Layouts/`: Folder tempat menyimpan file laporan Crystal Report (`.rpt`) Anda.

## Menambah Template di Production

Anda dapat menambah atau mengganti file `.rpt` di server production **tanpa harus build ulang** aplikasi. Aplikasi akan memindai folder secara dinamis setiap kali ada request.

### Panduan Menambah Template:

1.  **Struktur Folder**: Simpan file `.rpt` Anda di dalam sub-folder yang sesuai di bawah folder `Layouts` (misal: `Layouts/SO/`).
2.  **Izin Akses**: Pastikan user yang menjalankan IIS (AppPool) memiliki izin **Read** ke file baru tersebut.
3.  **Pengaturan Visual Studio**: Agar file `.rpt` otomatis ikut saat dideploy di masa depan:
    - Masukkan file ke dalam project.
    - Klik kanan file > **Properties**.
    - Set **Build Action** ke `Content`.
    - Set **Copy to Output Directory** ke `Copy if newer`.

## Keamanan Data

Proyek ini memisahkan rahasia (secrets) menggunakan file `appSettings` eksternal. **JANGAN PERNAH** memasukkan file `web.secrets.config` ke dalam version control.
