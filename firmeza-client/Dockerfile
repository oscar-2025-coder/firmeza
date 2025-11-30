# Usar Node 20 (más reciente y común en caché)
FROM node:20-slim

WORKDIR /app

# Copiar archivos de dependencias
COPY package*.json ./

# Instalar dependencias
RUN npm install

# Copiar código fuente
COPY . .

# Construir para producción
RUN npm run build

# Instalar servidor HTTP simple
RUN npm install -g serve

# Exponer puerto 80
EXPOSE 80

# Servir la aplicación construida
CMD ["serve", "-s", "dist", "-l", "80"]
