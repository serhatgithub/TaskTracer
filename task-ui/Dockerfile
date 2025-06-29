# =================================================================
# AŞAMA 1: REACT UYGULAMASINI BUILD ETMEK (BUILDER)
# =================================================================
FROM node:18-alpine AS builder

# Build sırasında kullanılacak ortam değişkenini (ARG) bildir
ARG REACT_APP_API_BASE_URL

# Gelen ARG'ı bir environment variable (ENV) olarak ayarla ki 'npm run build' kullanabilsin
ENV REACT_APP_API_BASE_URL=$REACT_APP_API_BASE_URL

WORKDIR /app

# Önce sadece package dosyalarını kopyala (Daha hızlı build için)
COPY package*.json ./

# Bağımlılıkları yükle
RUN npm install

# Geri kalan tüm kodları kopyala
COPY . .

# Uygulamayı build et
RUN npm run build


# =================================================================
# AŞAMA 2: ÜRETİM ORTAMI (NGINX)
# =================================================================
FROM nginx:stable-alpine

# Build edilmiş statik dosyaları NGINX'in sunacağı klasöre kopyala
COPY --from=builder /app/build /usr/share/nginx/html

# Önbelleği kapatan özel NGINX yapılandırmamızı kopyala
COPY nginx.conf /etc/nginx/conf.d/default.conf

# 80 portunu dışarıya aç
EXPOSE 80

# NGINX'i çalıştır
CMD ["nginx", "-g", "daemon off;"]