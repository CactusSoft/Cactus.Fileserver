# Cactus.Fileserver 
[ ![Download](https://travis-ci.com/CactusSoft/Cactus.Fileserver.svg?branch=develop) ](https://travis-ci.com/CactusSoft/Cactus.Fileserver)
[ ![Download](https://codecov.io/gh/CactusSoft/Cactus.Fileserver/graph/badge.svg) ](https://codecov.io/gh/CactusSoft/Cactus.Fileserver)

Simple lib for fast building your own file storage microservice. Implemented as OWIN handler for Katana as welll as Asp.NetCore.

Look at LocalFileserverExample for details.

Features:
- HTTP POST (multipart & not) to upload a file
- HTTP DELETE to drop a file
- Supporting S3 & Azure BLOB storages. Returns direct URL to stored file.
- Flexible piplene for new adding files
- Supports local filesystem storage for simple projects & dev environment 
