FROM nginx:alpine

COPY /oauth-client/nginx.conf /etc/nginx/nginx.conf

WORKDIR /usr/share/nginx/html
COPY /oauth-client/dist/ .