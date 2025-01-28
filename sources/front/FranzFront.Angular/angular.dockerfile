# Stage 1: Build the Angular application
FROM node:16 AS build

WORKDIR /app

# Install dependencies
COPY ./FranzFront.Angular/package*.json ./
RUN npm install

# Copy the rest of the files and build
COPY ./FranzFront.Angular/ ./
RUN npm run build --prod

# Stage 2: Serve the Angular app using Nginx
FROM nginx:alpine

# Copy the build output to Nginx
COPY --from=build /app/dist/ /usr/share/nginx/html

# Expose the port for Nginx
EXPOSE 80

# Start Nginx server
CMD ["nginx", "-g", "daemon off;"]
