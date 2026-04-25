variable "subscription_id" {
  description = "Azure Subscription ID where resources will be deployed."
  type        = string
}

variable "resource_group_name" {
  description = "Name of the Azure Resource Group."
  type        = string
  default     = "meetup-2504-rg"
}

variable "location" {
  description = "Azure region where resources will be created."
  type        = string
  default     = "eastus"
}

variable "prefix" {
  description = "Short prefix used to name all resources (e.g. meetup2504)."
  type        = string
  default     = "meetup2504"
}

variable "acr_name" {
  description = "Name of the Azure Container Registry. Must be globally unique, 5-50 alphanumeric characters."
  type        = string
  default     = "meetup2504acr"
}

variable "container_app_name" {
  description = "Name of the Azure Container App."
  type        = string
  default     = "meetup2504-app"
}

variable "image_tag" {
  description = "Tag of the container image to deploy (e.g. latest or a git SHA)."
  type        = string
  default     = "latest"
}
