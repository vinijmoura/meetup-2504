output "resource_group_name" {
  description = "Name of the created Resource Group."
  value       = azurerm_resource_group.rg.name
}

output "acr_login_server" {
  description = "Login server of the Azure Container Registry (e.g. meetup2504acr.azurecr.io)."
  value       = azurerm_container_registry.acr.login_server
}

output "acr_name" {
  description = "Name of the Azure Container Registry."
  value       = azurerm_container_registry.acr.name
}

output "container_app_name" {
  description = "Name of the deployed Azure Container App."
  value       = azurerm_container_app.app.name
}

output "container_app_url" {
  description = "Public HTTPS URL of the Container App."
  value       = "https://${azurerm_container_app.app.latest_revision_fqdn}"
}

output "log_analytics_workspace_id" {
  description = "Resource ID of the Log Analytics Workspace."
  value       = azurerm_log_analytics_workspace.law.id
}
