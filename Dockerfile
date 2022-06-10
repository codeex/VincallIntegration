FROM harbor.comm100dev.io/comm100-internal/aspnet:3.1
COPY  release /release
WORKDIR /release
