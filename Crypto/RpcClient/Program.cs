using Grpc.Net.Client;

const string localhost = "https://localhost:7247";

var channcel = GrpcChannel.ForAddress(localhost);