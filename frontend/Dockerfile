FROM node:20-alpine

WORKDIR /app

COPY package*.json ./

RUN npm install --legacy-peer-deps

COPY . .

ARG NEXT_PUBLIC_API_URL_CLIENT

ENV NEXT_PUBLIC_API_URL_CLIENT=$NEXT_PUBLIC_API_URL_CLIENT

RUN npm run build

EXPOSE 3000

CMD ["npm","start"]