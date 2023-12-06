dotnet build
sudo docker build -t flamehorizon/flight-scanner-arm64:latest . 
sudo docker login -u flamehorizon
sudo docker push flamehorizon/flight-scanner-arm64:latest
