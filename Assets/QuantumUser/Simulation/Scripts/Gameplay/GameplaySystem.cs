using Quantum.Prototypes;
using Quantum.Scripts.Weapon;
using UnityEngine;
using UnityEngine.Scripting;

namespace Quantum
{
	[Preserve]
	public unsafe class GameplaySystem : SystemMainThread, ISignalOnPlayerAdded
	{
		
		// SYSTEM
		public override void OnInit(Frame frame)
		{
			
		}
		
		
		public override void Update(Frame frame)
		{
			var gameplay = frame.Unsafe.GetPointerSingleton<Gameplay>();
			gameplay->Update(frame);
		}
		
		private EntityRef SpawnPlayer(Frame frame, RuntimePlayer playerData)
		{
			var playerCount = frame.ComponentCount<PlayerMovement>();
			
			if (playerData == null || playerData.playerPrototype.Id.IsValid == false)
			{
				Debug.LogError($"Cannot spawn player. Invalid player data.");
				return default;
			}
			
			//Create player
			var playerPrototype = frame.FindAsset<EntityPrototype>(playerData.playerPrototype.Id);
			var playerMoveEntity = frame.Create(playerPrototype);
			
			var playerMovement = frame.Unsafe.GetPointer<PlayerMovement>(playerMoveEntity);
			var playerAttack = frame.Unsafe.GetPointer<PlayerAttack>(playerMoveEntity);
			
			playerMovement->MyEntity = playerMoveEntity;
			playerMovement->SpawnIndex = playerCount;

			var config = frame.FindAsset<PlayerConfig>(playerAttack->playerConfig.Id);
			var playerStat = frame.Unsafe.GetPointer<PlayerStat>(playerMoveEntity);
			playerStat->PlayerNumber = playerCount;
			playerStat->PlayerHealth = config.maxHealth;
			playerStat->PlayerStamina = config.maxStamina;
			
			return playerMoveEntity;
		}
		
		// SIGNALS
		public void OnPlayerAdded(Frame frame, PlayerRef playerRef, bool firstTime)
		{
			var gameplay = frame.Unsafe.GetPointerSingleton<Gameplay>();
		
			var playerData = frame.GetPlayerData(playerRef);
			var startPositions = frame.ResolveList(gameplay->StartPositions);
			
			var playerEntity = SpawnPlayer(frame, playerData);
			
			var playerMovement = frame.Unsafe.GetPointer<PlayerMovement>(playerEntity);
			var playerStat = frame.Unsafe.GetPointer<PlayerStat>(playerEntity);
			
			var startTransform = frame.Unsafe.GetPointer<Transform3D>(startPositions[playerMovement->SpawnIndex]);
			playerMovement->PlayerRef = playerRef;
			playerMovement->PlayerType = EPlayerType.Player;
				
			playerMovement->Teleport(frame, playerEntity, startTransform);
			playerStat->SetData(frame, playerEntity);
			
			if (!gameplay->SpawnAI)
			{
				var aiPlayerCount = frame.RuntimeConfig.aiPlayerCount;

				for (var i = 0; i < aiPlayerCount; i++)
				{
					var aiEntity = SpawnPlayer(frame, playerData);
					var aiPlayer = frame.Unsafe.GetPointer<AIPlayer>(aiEntity);
					var aiMovement = frame.Unsafe.GetPointer<PlayerMovement>(aiEntity);
					var aiPlayerStat = frame.Unsafe.GetPointer<PlayerStat>(aiEntity);
					
					aiPlayer->IsActive = true;
					
					var startTransformAI = frame.Unsafe.GetPointer<Transform3D>(startPositions[aiMovement->SpawnIndex]);
					aiMovement->PlayerRef = PlayerRef.None;
					aiMovement->PlayerType = EPlayerType.AI;
					aiMovement->Teleport(frame, aiEntity, startTransformAI);
					
					aiPlayerStat->SetData(frame, aiEntity);
				}
			}
	
			
			gameplay->OnPlayerConnected(frame);
		}

	
	}
}






