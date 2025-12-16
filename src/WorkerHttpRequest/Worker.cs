using System.Runtime.InteropServices;

namespace WorkerHttpRequest;

public class Worker(ILogger<Worker> logger,
    IConfiguration configuration) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Iniciando a execucao do Job...");
        logger.LogInformation($"Versao do .NET em uso: {RuntimeInformation
            .FrameworkDescription} - Ambiente: {Environment.MachineName} - Kernel: {Environment
            .OSVersion.VersionString}");
        var endpointRequest = configuration["EndpointRequest"];
        logger.LogInformation($"URL para envio da requisicao: {endpointRequest}");
        var waitingTimeSeconds = configuration.GetValue<int>("WaitingTimeInSeconds");
        logger.LogInformation($"Tempo de espera entre as requisicoes: {waitingTimeSeconds} segundos");

        using var httpClient = new HttpClient();
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                logger.LogInformation("Enviando requisicao em: {time:yyyy-MM-dd HH:mm:ss}", DateTimeOffset.UtcNow.AddHours(-3));
                var response = await httpClient.GetAsync(endpointRequest);
                logger.LogInformation("Requisicao recebida em: {time:yyyy-MM-dd HH:mm:ss}", DateTimeOffset.UtcNow.AddHours(-3));
                logger.LogInformation($"Response Status Code = {(int)response.StatusCode} {response.StatusCode}");
                response.EnsureSuccessStatusCode();

                logger.LogInformation("Notificacao enviada com sucesso!");
                logger.LogInformation($"Dados recebidos = {await response.Content.ReadAsStringAsync()}");
                logger.LogInformation("Processamento da requisicao executado com sucesso!");
            }
            catch (Exception ex)
            {
                logger.LogError($"Erro durante a execucao do Job: {ex.Message}");
                Environment.ExitCode = 1;
            }
            await Task.Delay(waitingTimeSeconds * 1000, stoppingToken);
        }
    }
}
