variable "gcp_region" {
  description = "GCP region where resources will be created"
  type        = string
}

variable "vpc_name" {
  description = "Name of the VPC network"
  type        = string
  default     = "main-vpc"
}

variable "public_subnet_cidr" {
  description = "CIDR block for the public subnet"
  type        = string
  default     = "10.0.1.0/24"
}

variable "private_subnet_cidr" {
  description = "CIDR block for the private subnet"
  type        = string
  default     = "10.0.2.0/24"
}

variable "enable_ssh" {
  description = "Enable SSH access (true/false)"
  type        = bool
  default     = true
}

variable "allowed_ssh_ranges" {
  description = "Allowed source ranges for SSH"
  type        = list(string)
  default     = ["0.0.0.0/0"]
}

variable "microservice_ports" {
  description = "List of TCP ports to open for microservices"
  type        = list(string)
  default     = ["8080"]
}
