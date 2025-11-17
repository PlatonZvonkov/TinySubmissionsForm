# Part 1
 BE:
# .Net9 + ASP.NET Core
# REST-service based on Clean Architecture
#
 FE:
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


# Part 2
Considering handling large files (~100mb) with high rates - this will require decoupling storage from the database. This approach ensures the database remains fast for querying metadata, while a highly scalable Object Storage system manages the large files.
How would i approach this issue:
The architecture will be shifted to use a dedicated cloud service for binary data and will employ the Pre-Signed URL pattern, which offloads the heavy data transfer from the Web API and handles security and authorization efficiently.

# This will require new components in the system:
1) Object Storage (existing service) like AWS S3, Azure Blob, Google Cloud. This is where the 100MB files will reside. It's highly scalable, durable, and cost-effective from the box solution.
2) New IAttachmentStorageService type and implementation in the Infrastructure layer responsible for interacting with the chosen Storage. It would contains methods like GenerateUploadUrl(string key), GenerateDownloadUrl(string key), and DeleteAttachment(string key).
3) Attachment Upload Flow - Meaning the client never uploads the file directly through the Web API.
Instead client code request (Web API): The client sends a request to the API (POST /api/submissions/upload-url) requesting permission to upload a specific file (identified by its expected size/type).
4) Then API generates Url by calling the IAttachmentStorageService, which generates a time-limited, secure signed upload URL from the Storage.
API returns generated URL and the unique StorageKey to the client.
5) Client code uses the signed URL to upload the 100MB file directly to the Object Storage. The API is bypassed entirely for the large data transfer.
6) Once the upload is confirmed, the client submits the final form data, including the unique StorageKey (not the file content itself), to the POST /api/submissions endpoint.
7) The unique StorageKey and attachment metadata are saved in the relational database ( new SubmissionAttachments table).

# OR 

1) Client code request a download link for a specific attachment (GET /api/submissions/{id}/attachments/{key}/download-url).
2) API checks the database to verify the user is authorized to access the attachment associated with the submission ID.
3) API calls the IAttachmentStorageService, which generates a time-limited, secure Download URL from the  Object Storage.
4) API returns the download URL.
5) Client code uses this temporary URL to download the file directly from the Storage.
