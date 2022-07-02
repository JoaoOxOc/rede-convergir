# redeconvergir-web
website and web services of Rede Convergir in Portugal


## How to run it
### for micro frontend's monorepo
- First, explanation of frontend's root folder `package.json`:
	- `npm run bootstrap` (on the frontend's root folder): triggers the command `lerna bootstrap --hoist` responsible for moving all package dependencies to the root `node_modules` folder
	- `npm run tsc` (on the frontend's root folder): it triggers the command `lerna run tsc --stream` compiling every monorepo package built with typescript
	- `npm run:home:dev` (on the frontend's root folder): it will start the "home" NextJS project. Contains a bigger command inside - lerna scope is the project that should run and stream parameter is the project's package script to run
	- `npm run run:backoffice:dev`(on the frontend's root folder): it will start the "backoffice" NextJS project. Also the same commands as the previous line explanation
	
- How to run frontends in dev environment:
	- first, run `npm install lerna` command
	- after lerna is installed, run `npm run bootstrap` command just to verify all the dependencies
	- create a `.env.local` file in the backoffice project folder and insert the following parameters:
		- `NEXT_PUBLIC_API_BASE_URL` - the base URL for the backend gateway
		- `NEXT_PUBLIC_AUTH_URL` - the next-auth url like "http://localhost:3000/admin/api/auth"
		- `NEXT_PUBLIC_CLIENT_ID` - the client ID authorized to access backend API's
		- `NEXT_PUBLIC_CLIENT_SECRET` - the client secret for authorization to access backend API's
		- `NEXT_PUBLIC_HOME_URL` - the URL of the home frontend project
		- `NEXTAUTH_SECRET` - the RSA secret used by next-auth to encrypt cookies
		- `NEXTAUTH_URL` - the next-auth url like "http://localhost:3000/admin/api/auth"
		- `AUTH_API_URL` - the base URL for the backend gateway
		- `AUTH_API_CLIENT_ID` - the client ID authorized to access backend API's and used by next-auth server side methods
		- `AUTH_API_CLIENT_SECRET` - the client secret for authorization to access backend API's and used by next-auth server side methods
	- just run "home" or "backoffice" commands (`npm run run:backoffice:dev` for example, depending on which project you want to start
	
- How to install packages in a specific project:
	- example: `lerna add styled-components --scope=@redeconvergir/generic_header`
	- last, run `npm run bootstrap` to remove duplicated NPM packages
	
- How to run in Docker:
	- for home project: use the dockerfile `frontoffice.dockerfile` inside `frontends` folder; from this folder in CMD, run: `docker build -f .\frontoffice.dockerfile -t tp_front .` (or for debug `docker build -f .\frontoffice.dockerfile -t tp_front . --progress=plain --no-cache`) then `docker run -p 5050:3000 tp_front`
	- for backoffice project: use the dockerfile `backoffice.dockerfile` inside `frontends` folder; from this folder in CMD, run: `docker build -f .\backoffice.dockerfile -t tp_front .` (or for debug `docker build -f .\backoffice.dockerfile -t tp_front . --progress=plain --no-cache`) then `docker run -p 5040:3000 tp_backoffice`

### for micro services
- First, explanation of microservices's root folder:
	- tpgateway: .NET 5 project; responsible for API routing; has authorization and authentication settings for each route
	- UserService: .NET 6 project; responsible for user registration; emits JWT tokens for authorization and authentication
	- NewsService: DJango 2.2 project with Python v3.9; responsible for media content like news, events, blogging, petitions, etc.
	- NotificationsService: .NET 6 project; responsible to send notifications to distinct apps (SignalR, Slack, etc.)
	- EmailService: .NET 6 project; responsible for email templates configurations and email send
	- ContentManageService: .NET 6 project; responsible for website CMS
	
- How to run microservices in dev environment:
	- use docker compose: `docker-compose  -f "\tp_webservices\docker-compose.yml" -f "\tp_webservices\docker-compose.override.yml" -p dockercompose_tpwebservices --ansi never build`

## How to deploy it
- Start with the API's: change Ocelot route JSON files in the `deployback` repository to match kubernetes routes. The kubernetes related deploy action will start automatically
- Store client credentials: get the frontend ones from the production database
- Change frontend's client credentials: in the secrets or in the ENV files
- Deploy frontends: push to `deployfront` repository and the kubernetes related deploy action will start automatically
