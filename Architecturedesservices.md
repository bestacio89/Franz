# :moneybag: Architecture des Systèmes Économie & Items (MOBA Backend)

Ce document décrit l’ajout et la séparation de deux nouveaux systèmes essentiels dans l’architecture globale :

- ItemService (système d’équipement gameplay)
- EconomyService (monétisation + progression)

Ces systèmes sont strictement séparés des services de simulation et de contenu.

---

# :brain: Philosophie générale

Le système est structuré autour d’un principe fondamental :

> :exclamation: Aucun système externe ne doit résoudre du gameplay directement.

Tous les systèmes économiques ou d’équipement fournissent uniquement des **données et modificateurs**, tandis que la résolution finale est toujours effectuée par le MatchService.

---

# :crossed_swords: 1. ItemService (Système d’objets gameplay)

## :dart: Objectif
Gérer les objets équipables qui influencent légèrement les performances des héros en match.

---

## :jigsaw: Responsabilités

- Définition des items
- Stats associées aux items
- Effets passifs ou conditionnels
- Rareté des items
- Synergies potentielles (optionnel futur)

---

## :pushpin: Principe clé

> Les items ne prennent jamais de décisions de gameplay.

Ils fournissent uniquement des **modificateurs numériques ou contextuels**.

---

## :brain: Exemple de logique

- +Attack Speed
- +Armor
- +Cooldown Reduction
- Résistances spécifiques
- Bonus situationnels

---

## :warning: Contraintes importantes

- Aucun calcul de combat
- Aucun effet direct sur les HP/mana
- Aucun accès au runtime MatchService
- Aucun comportement actif

---

## :arrows_counterclockwise: Utilisation en match

1. UserService fournit les items équipés
2. MatchService récupère les données des items
3. MatchService applique les modificateurs dans sa pipeline interne

---

# :moneybag: 2. EconomyService (Système monétaire & progression)

## :dart: Objectif
Gérer toute la logique économique du jeu, séparée du gameplay.

---

## :jigsaw: Responsabilités

### :currency_exchange: Monnaies
- Monnaie premium (payante)
- Monnaie in-game (gagnée en match)

### :shopping_cart: Boutique
- Achat de héros
- Achat de skins
- Bundles
- Offres temporaires

### :chart_with_upwards_trend: Progression économique
- Récompenses de match
- Missions quotidiennes
- Bonus d’événements

### :lock: Validation et sécurité
- Historique de transactions
- Anti-fraude
- Cohérence des inventaires

---

## :pushpin: Principe clé

> L’économie ne doit jamais influencer directement le gameplay.

---

## :brain: Exemple de flux

1. Le joueur gagne un match
2. MatchService envoie les récompenses
3. EconomyService crédite la monnaie
4. UserService met à jour l’inventaire

---

## :warning: Contraintes importantes

- Aucun lien avec les formules de combat
- Aucun impact sur MatchService
- Aucun système de RNG pay-to-win
- Aucun avantage gameplay lié à l’achat

---

# :brain: Interaction entre services

## :arrows_counterclockwise: Flux global

```text id="flow1"
UserService
   ↓
(ownership / inventory)

ItemService
   ↓
(equipment definitions)

EconomyService
   ↓
(currency + purchases)

MatchService
   ↓
(runtime simulation ONLY)`


et voici les deux derniers services