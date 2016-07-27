# Cactus.Fileserver
Simple lib for fast building your own file storage microservice. Implemented as OWIN handler for Katana as welll as Asp.Net Core, so could be easy integrated into existing app pipeline if need.

Look at LocalFileserverExample for details.

It accepts:
- multipart POST /file to upload any file type and returns file url in Location header
- GET /file/{name} - to get a file
- GET /file/{name}/info or /file/{name}?info - to get a file information
- DELETE /file/{name} to drop the file

It does:
- Storing file in local file system
- Preprocessing image content 
