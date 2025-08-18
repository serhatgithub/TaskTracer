# TaskTracer 

TaskTracer, mikroservis tabanlı, Docker üzerinde konteynerleştirilen ve AWS üzerinde bir CI/CD hattı ile dağıtılan modern bir **Görev Yönetim Uygulaması**dır.
---

## Mimari Genel Bakış

Proje, birbirinden bağımsız olarak geliştirilebilen ve dağıtılabilen mikroservislerden oluşmaktadır. Servisler arası iletişim bir API Gateway üzerinden sağlanırken, veri kalıcılığı için PostgreSQL veritabanı kullanılmaktadır.

**Backend (ASP.NET Core Microservices)**
  - **AuthService** → JWT tabanlı kimlik doğrulama
  - **UserService** → Kullanıcı yönetimi
  - **TaskService** → Görev CRUD işlemleri
  - **API Gateway (Ocelot)** → Servisler arası yönlendirme
  - **PostgreSQL (EF Core)** → Kalıcı veri yönetimi  

**Frontend (React.js SPA)**
  - Kanban tabanlı görev panosu
  - Login / Register ekranları
  - Kullanıcı paneli  
  - **Nginx** üzerinden servis edilip API Gateway’e reverse proxy yönlendirmesi

**DevOps & Cloud**
  - **Docker + Docker Compose** ile konteynerleştirme
  - **GitHub Actions** ile CI/CD otomasyonu
  - **AWS EC2** üzerinde canlı dağıtım
  - **Nginx** ile reverse proxy & load balancing
  - Gelecekte **Kubernetes (EKS)** ile ölçeklenebilirlik
---

## Kullanılan Teknolojiler

| Katman | Teknoloji / Araçlar |
| :--- | :--- |
| **Backend** | ASP.NET Core (.NET 9), C#, Entity Framework Core, PostgreSQL, Ocelot API Gateway |
| **Frontend** | React.js (ES6+), Axios, CSS3, Nginx |
| **Konteyner** | Docker, Docker Compose |
| **CI/CD** | GitHub Actions |
| **Bulut** | AWS (EC2, S3, IAM, VPC, Route53, CloudFront) |
| **Güvenlik** | JWT, HTTPS, CORS, BCrypt password hashing |
| **İzleme/Debug** | Swagger, Chrome DevTools |

---

## Kurulum ve Çalıştırma

Projeyi yerel makinenizde çalıştırmak için aşağıdaki adımları izleyebilirsiniz.

1.  **Depoyu Klonlayın**
    ```bash
    git clone https://github.com/serhatgithub/TaskTracer.git
    cd TaskTracer
    ```

2.  **Docker Compose ile Servisleri Ayağa Kaldırın**
    Bu komut, gerekli Docker imajlarını oluşturacak ve tüm servisleri başlatacaktır.
    ```bash
    docker-compose up --build
    ```

3.  **Servislere Erişin**
    - **Frontend (React + Nginx)**: `http://localhost:3000`
    - **API Gateway**: `http://localhost:5000`
    - **PostgreSQL DB**: `localhost:5432`

---

## CI/CD Süreci

Proje, **GitHub Actions** ile otomatize edilmiş bir CI/CD hattına sahiptir. `main` branch'ine yapılan her push, aşağıdaki adımları tetikler:

1.  **Checkout & Build**: Kodun güncel hali alınır ve .NET ile React projeleri derlenir.
2.  **Docker Build & Push**: Her servis için Docker imajları oluşturulur ve bir konteyner kayıt defterine (Docker Hub / ECR) gönderilir.
3.  **Deploy to AWS**: AWS EC2 üzerindeki sunucuya bağlanılarak güncel imajlar çekilir ve konteynerler yeniden başlatılır.
4.  **Verification**: `Health-check` endpoint'leri üzerinden servislerin sağlıklı çalışıp çalışmadığı kontrol edilir.

---

## Özellikler

- **Kullanıcı Yönetimi**: JWT ile güvenli kullanıcı girişi ve kayıt işlemleri.
- **Kanban Panosu**: Görevleri "Todo", "Doing" ve "Done" gibi sütunlar arasında sürükleyip bırakarak yönetme.
- **Görev CRUD**: Yeni görev ekleme, mevcut görevleri güncelleme, silme ve sıralama.
- **Profil Yönetimi**: Kullanıcı adı ve şifre gibi profil bilgilerini güncelleme.
- **Çok Kullanıcılı Destek**: Sistemin birden fazla kullanıcı tarafından aynı anda kullanılabilmesi (multi-tenant).


## Lisans

Bu proje eğitim ve portföy amaçlı olarak geliştirilmiştir. Lisans bilgisi daha sonra eklenecektir.

---
