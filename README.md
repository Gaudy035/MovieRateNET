# MovieRate

Full-stack movie rating web application built with **Next.js, .NET and PostgreSQL**.
It is a .NET version of my MovieRate project: https://github.com/Gaudy035/MovieRate

## Features

- **User authentication** - User authentication using JWT and HTTP only cookies.
- **Movie browsing** - Filtering movies in database by title.
- **Review system** - Viewing and writing reviews for movies.
- **Interactive documentation** - Interactive docs generated via Swagger UI available at `/docs`.

## Tech stack

- **Frontend** (Port:3000) - Next.js, TypeScript, tailwind.css
- **Backend** (Port:3001) - .NET 8 / ASP.NET Core Web API, Entity Framework Core
- **Database** (Port:5433) - PostgreSQL

## Setting up

### Environment variables

Each required file has a corresponding .example showing how the file should look like

#### root .env

Variables used by database container in Docker

| Variable          | Description                            |
| ----------------- | -------------------------------------- |
| POSTGRES_USER     | User used by docker to access database |
| POSTGRES_PASSWORD | Database user password                 |
| POSTGRES_DB       | Name of the database used by Docker    |

#### frontend/.env

Frontend variables

| Variable                   | Description                                   |
| -------------------------- | --------------------------------------------- |
| API_URL_SERVER             | URL used by server components to call backend |
| NEXT_PUBLIC_API_URL_CLIENT | URL used by client components to call backend |

#### backend/appsettings.json and backend/appsettingsDevelopment.json

Backend variables are structured as a .json file

For database connection you need to provide a connection string

```json
"ConnectionStrings": {
    "DbConnection": "Host=<HOST>; Port=<PORT>; Database=<DB>; Username=<DB_USER>; Password=<PASS>"
},
```

You need to replace the variables in <>

| Variable | Description                                       |
| -------- | ------------------------------------------------- |
| HOST     | Host where the database server runs               |
| PORT     | Port on which database server runs                |
| DB       | Name of the database used by app                  |
| DB_USER  | User that backend will use to connect to database |
| PASS     | Database user password                            |

Besides database variables you also need to provide:

```json
  "Cors": {
    "AllowedOrigins": ["<ORIGIN_URL>"]
  },
  "Jwt": {
    "Key": "<JWT_KEY>"
  }
```

| Variable   | Description                                        |
| ---------- | -------------------------------------------------- |
| JWT_KEY    | Secret key used by JWT, minimum 32 characters long |
| ORIGIN_URL | URL of frontend                                    |

### Starting

When starting for the first time run:

```bash
docker-compose up --build
```

On later start-ups run

```bash
docker-compose up
```

To stop the app run

```bash
docker-compose down
```
