module "networking" {
  source = "./modules/networking"
}

module "kafka" {
  source = "./modules/kafka"
  use_msk = var.use_msk
}

module "database" {
  source        = "./modules/database"
  database_type = var.database_type
  multi_db      = var.multi_db
}

module "ecs" {
  source = "./modules/ecs"
  use_eks = var.use_eks
}
