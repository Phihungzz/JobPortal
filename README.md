# Job Portal

## Xây dựng cổng thông tin việc làm trực tuyến tích hợp AI

## Giới thiệu

Là ứng dụng nhằm kết nối **ứng viên** với **nhà tuyển dụng**, đồng thời hỗ trợ tối ưu hóa quy trình tuyển dụng thông qua việc phân tích và lọc hồ sơ ứng viên bằng AI.

Hệ thống được phát triển trên nền tảng **ASP.NET Core**, sử dụng **SQL Server** làm cơ sở dữ liệu và tích hợp **Google Gemini API** để đánh giá mức độ phù hợp của CV với yêu cầu tuyển dụng.

---

## Mục tiêu

- Xây dựng hệ thống web kết nối ứng viên với nhà tuyển dụng.
- Quản lý thông tin tuyển dụng và hồ sơ ứng viên.
- Tích hợp AI để tự động đánh giá và lọc CV.
- Hỗ trợ doanh nghiệp rút ngắn thời gian tuyển dụng.
- Nâng cao trải nghiệm tìm việc cho ứng viên.

---

## Đối tượng

- Ứng viên
- Nhà tuyển dụng
- Quy trình tuyển dụng

---

## Công nghệ sử dụng bắt buộc

### Backend

- C#
- ASP.NET Core

### Database

- Microsoft SQL Server

### AI

- Google Gemini API

### Thư viện

- Microsoft.EntityFrameworkCore
- iText.Kernel
- System.Text.Json

---

## Quy trình lọc CV bằng AI

Hệ thống thực hiện quy trình đánh giá CV theo các bước sau:

1. Nhà tuyển dụng tạo yêu cầu tuyển dụng.
2. Hệ thống xây dựng Prompt dựa trên mô tả công việc.
3. Ứng viên tải CV lên hệ thống.
4. AI trích xuất thông tin về kỹ năng, kinh nghiệm và học vấn từ CV.
5. AI tính toán **Match Score** giữa CV và yêu cầu tuyển dụng.
6. Hệ thống sắp xếp và lọc các ứng viên phù hợp.

```
Job Description
        │
        ▼
 Create Prompt
        │
        ▼
 Gemini API
        │
        ▼
 Extract CV Information
        │
        ▼
 Calculate Match Score
        │
        ▼
 Candidate Ranking
```

---

## Chức năng chính

### Ứng viên

- Đăng ký, đăng nhập
- Quản lý hồ sơ cá nhân
- Upload CV
- Tìm kiếm việc làm
- Ứng tuyển trực tuyến
- Theo dõi trạng thái ứng tuyển

### Nhà tuyển dụng

- Đăng ký, đăng nhập
- Quản lý doanh nghiệp
- Đăng tin tuyển dụng
- Quản lý ứng viên
- Lọc CV bằng AI
- Xem điểm Match Score

---

## Kiến trúc hệ thống

```
Frontend (ASP.NET Core MVC)
            │
            ▼
Business Logic
            │
            ▼
AI Service (Gemini API)
            │
            ▼
SQL Server Database
```

---

## Yêu cầu hệ thống

- .NET SDK
- Visual Studio 2022
- SQL Server
- Gemini API Key

---

## Cài đặt

Clone project

```bash
git clone ttps://github.com/Phihungzz/JobPortal.git
```

Di chuyển vào thư mục project

```bash
cd JobPortal
```

Thêm API Key của Gemini vào:

```json
"Gemini": {
  "ApiKey": "YOUR_API_KEY"
}
```

Chạy project

```bash
dotnet run
```

---

Author: *Nguyễn Lê Phi Hùng*.
