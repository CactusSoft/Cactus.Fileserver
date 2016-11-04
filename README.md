# Cactus.Fileserver
Simple lib for fast building your own file storage microservice. Implemented as OWIN handler for Katana as welll as Asp.NetCore.

Look at LocalFileserverExample for details.

Features:
- HTTP POST (multipart & not) to upload a file
- HTTP DELETE to drop a file
- Supporting Azure BLOB storage & returns direct URL's
- Flexible image pre-processing
- Flexible piplene for new adding files
- Supports StaticFiles middleware for dev envirounment & simple cases
