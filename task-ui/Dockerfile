# Aşama 1: Build (Node.js ortamında React uygulamasını build et)
# Bu aşama, React uygulamasını statik dosyalara dönüştürür.
FROM node:18-alpine AS build
WORKDIR /app

# package.json ve package-lock.json dosyalarını kopyala
# Bu, katmanlı önbellekleme sayesinde, bağımlılıklar değişmediyse 'npm install' adımını atlar.
COPY package.json ./
COPY package-lock.json ./

# Bağımlılıkları yükle
RUN npm install

# Tüm kaynak kodunu kopyala
COPY . .

# React uygulamasını build et, statik dosyalar /app/build klasörüne oluşturulacak
RUN npm run build

# Aşama 2: Production (Hafif bir Nginx web sunucusu ile statik dosyaları sun)
# Bu aşama, production imajını oluşturur.
FROM nginx:stable-alpine

# Build aşamasında oluşturulan statik dosyaları Nginx'in web root klasörüne kopyala
COPY --from=build /app/build /usr/share/nginx/html

# İsteğe bağlı: Nginx için özel bir konfigürasyon dosyası kopyalayabiliriz.
# Bu, React Router gibi client-side routing kütüphaneleri için gereklidir.
# Eğer /tasks gibi bir URL'yi yenilerseniz 404 hatası almamak için tüm istekleri index.html'e yönlendirir.
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Nginx'in dinleyeceği port (varsayılan 80)
EXPOSE 80

# Nginx'i başlat
CMD ["nginx", "-g", "daemon off;"]