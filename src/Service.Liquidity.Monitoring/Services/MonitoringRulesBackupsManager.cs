using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Service.Liquidity.Monitoring.Domain.Interfaces;
using Service.Liquidity.Monitoring.Grpc;
using Service.Liquidity.Monitoring.Grpc.Models.Backups;

namespace Service.Liquidity.Monitoring.Services
{
    public class MonitoringRulesBackupsManager : IMonitoringRulesBackupsManager
    {
        private readonly ILogger<MonitoringRulesBackupsManager> _logger;
        private readonly IMonitoringRulesBackupsStorage _backupsStorage;
        private readonly IMonitoringRulesStorage _monitoringRulesStorage;

        public MonitoringRulesBackupsManager(
            ILogger<MonitoringRulesBackupsManager> logger,
            IMonitoringRulesBackupsStorage backupsStorage,
            IMonitoringRulesStorage monitoringRulesStorage
        )
        {
            _logger = logger;
            _backupsStorage = backupsStorage;
            _monitoringRulesStorage = monitoringRulesStorage;
        }

        public async Task<GetMonitoringRulesBackupInfosResponse> GetInfosAsync(GetMonitoringRulesBackupInfosRequest request)
        {
            try
            {
                var infos = await _backupsStorage.GetInfosAsync();

                return new GetMonitoringRulesBackupInfosResponse
                {
                    Items = infos
                };
            }
            catch (Exception ex)
            {
                return new GetMonitoringRulesBackupInfosResponse
                {
                    IsError = true,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<AddOrUpdateMonitoringRulesBackupResponse> AddOrUpdateAsync(AddOrUpdateMonitoringRulesBackupRequest request)
        {
            try
            {
                await _backupsStorage.AddOrUpdateAsync(request.Item);

                return new AddOrUpdateMonitoringRulesBackupResponse();
            }
            catch (Exception ex)
            {
                return new AddOrUpdateMonitoringRulesBackupResponse
                {
                    IsError = true,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<GetMonitoringRulesBackupResponse> GetAsync(GetMonitoringRulesBackupRequest request)
        {
            try
            {
                var item = await _backupsStorage.GetAsync(request.Id);

                return new GetMonitoringRulesBackupResponse
                {
                    Item = item
                };
            }
            catch (Exception ex)
            {
                return new GetMonitoringRulesBackupResponse
                {
                    IsError = true,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<DeleteMonitoringRulesBackupResponse> DeleteAsync(DeleteMonitoringRulesBackupRequest request)
        {
            try
            {
                await _backupsStorage.DeleteAsync(request.Id);

                return new DeleteMonitoringRulesBackupResponse();
            }
            catch (Exception ex)
            {
                return new DeleteMonitoringRulesBackupResponse
                {
                    IsError = true,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<ApplyMonitoringRulesBackupResponse> ApplyAsync(ApplyMonitoringRulesBackupRequest request)
        {
            try
            {
                var backup = await _backupsStorage.GetAsync(request.Id);
                await _monitoringRulesStorage.AddOrUpdateAsync(backup.MonitoringRules);

                return new ApplyMonitoringRulesBackupResponse();
            }
            catch (Exception ex)
            {
                return new ApplyMonitoringRulesBackupResponse
                {
                    IsError = true,
                    ErrorMessage = ex.Message
                };
            }
        }
    }
}