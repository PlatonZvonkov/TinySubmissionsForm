# BE:
# .Net9 + ASP.NET Core
# REST-service based on Clean Architecture
#
# FE:
# Vue3 + Vite
# SinglePage UI for listing and searching the submitted objects from different forms

To run and view tests locally in real time, you need to:

  -  Clone the repo to your local machine

  -  Choose option to use or not to use InMemory DataBase in .appsettings.json file: UseEfInMemory with the value - true or false

  -  Launch backend by building it via IDE or console from base folder:

    Run the following commands:

       dotnet run --project TinyForm.WebAPI

   - Then frontend:

    cd frontend

    npm install

    npm install axios vee-validate yup

    npm run build 
