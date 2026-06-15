# use the official Bun image
FROM oven/bun:1 AS base
WORKDIR /app

# install dependencies into temp directory (cache for faster rebuilds)
FROM base AS install
RUN mkdir -p /temp/dev
COPY package.json bun.lock /temp/dev/
RUN cd /temp/dev && bun install --frozen-lockfile

# install production dependencies
RUN mkdir -p /temp/prod
COPY package.json bun.lock /temp/prod/
RUN cd /temp/prod && bun install --frozen-lockfile --production

# copy deps and source, then build
FROM base AS builder
COPY --from=install /temp/dev/node_modules node_modules
COPY . .

ARG NEXT_PUBLIC_API_URL_CLIENT
ENV NEXT_PUBLIC_API_URL_CLIENT=$NEXT_PUBLIC_API_URL_CLIENT
ENV NODE_ENV=production

RUN bun run build

# final image – only production deps + built output
FROM base AS release
COPY --from=install /temp/prod/node_modules node_modules
COPY --from=builder /app/public ./public
COPY --from=builder /app/.next/standalone ./
COPY --from=builder /app/.next/static ./.next/static
COPY --from=builder /app/package.json .

USER bun
EXPOSE 3000

CMD ["bun", "run", "start"]