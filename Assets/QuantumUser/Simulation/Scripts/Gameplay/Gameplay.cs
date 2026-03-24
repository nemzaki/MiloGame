using System;
using Photon.Deterministic;
using Quantum.Prototypes;
using System.Collections.Generic;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace Quantum
{
	
	public unsafe partial struct Gameplay
	{
		private static string[] prefixes = { "Lil", "Big", "Young", "DJ", "MC", "OG", "King", "Queen" };
		private static string[] words = { "Flex", "Savage", "Drip", "Ghost", "Heat", "Hustle", "Blade", "Cash", "Rider", "Storm", "Flow", "Wave" };
		private static string[] suffixes = { "Boy", "Girl", "Kid", "Man", "G", "Boss", "Baby", "Gang" };
		
		public void Update(Frame frame)
		{
			StateTime += frame.DeltaTime;
			
			if (State == EGameplayState.Waiting && StateTime >= frame.RuntimeConfig.waitingTime)
			{
				SetState(frame, EGameplayState.Start);
			}
			else if (State == EGameplayState.Start && StateTime >= frame.RuntimeConfig.startTime)
			{
				SetState(frame, EGameplayState.InProgress);
			}
			
			if (State == EGameplayState.Waiting)
			{
				SetState(frame, EGameplayState.Start);
			}
			else if (State == EGameplayState.InProgress)
			{
				UpdateRoundState(frame);
			}
			
			UpdatePlayerCount(frame);
			CheckConnections(frame);
			CheckCloseCombat(frame);
			
			CheckSlowMo(frame);
			HandleSlowMo(frame);
		}

		private void SetState(Frame frame, EGameplayState state)
		{
			if (State == state)
				return;
			
			State = state;

			if (State == EGameplayState.Waiting)
			{
				
			}
			else if (State == EGameplayState.Start)
			{
				
			}
			else if (State == EGameplayState.InProgress)
			{
				
			}
			else if (State == EGameplayState.Finished)
			{
				frame.Events.MatchFinished();
			}
		}
		
		public void OnPlayerConnected(Frame frame)
		{
			if (State != EGameplayState.None)
				return;

			SetState(frame, EGameplayState.Waiting);
		}

		private void UpdatePlayerCount(Frame frame)
		{
			TotalPlayers = frame.ComponentCount<PlayerMovement>();
		}

		#region ROUNDS
		private void UpdateRoundState(Frame frame)
		{
			RoundTimer += frame.DeltaTime;

			switch (RoundState)
			{
				case ERoundState.None:
					StartIntro();
					break;
				
				case ERoundState.Intro:
					if (RoundTimer >= 3)
					{
						BeginRound(frame);
					}
					break;
				
				case ERoundState.Active:
						ActiveRound(frame);
					break;
					
				case ERoundState.End:
					if (RoundTimer >= 2)
					{
						PrepareReset(frame);
					}
					break;
				
				case ERoundState.Reset:
					if (RoundTimer >= 3)
					{
						AdvanceRound(frame);
					}
					break;
			}
		}

		private void StartIntro()
		{
			RoundState = ERoundState.Intro;
			RoundTimer = 0;

			InitiateFight = true;
			CanFight = false;
			CurrentRound = 1;
		}

		private void BeginRound(Frame frame)
		{
			RoundState = ERoundState.Active;
			RoundTimer = 0;
			
			InitiateFight = false;
			CanFight = true;
			
			frame.Events.StartNewRound(CurrentRound);
		}

		private void ActiveRound(Frame frame)
		{
			if (!SuddenDeath)
			{
				InGameMatchTime += frame.DeltaTime;
			}

			CheckRoundEnd(frame);
			CheckMatchOver(frame);
				
			if (InGameMatchTime >= frame.RuntimeConfig.gameTime)
			{
				if (CurrentRound > 3)
				{
					SetState(frame, EGameplayState.Finished);
				}
			}
				
			//Reset Time for training
			if (frame.RuntimeConfig.training && InGameMatchTime <= 30)
			{
				InGameMatchTime = 180;
			}
		}
		
		private void CheckRoundEnd(Frame frame)
		{
			// ---------- SUDDEN DEATH RESOLUTION ----------
			if (SuddenDeath)
			{
				if(SuddenDeathResolved)
					return;
				
				PlayerStat* sdP1 = null;
				PlayerStat* sdP2 = null;

				var sdE1 = EntityRef.None;
				var sdE2 = EntityRef.None;
				
				foreach (var pair in frame.Unsafe.GetComponentBlockIterator<PlayerStat>())
				{
					if (sdE1 == EntityRef.None)
					{
						sdE1 = pair.Entity;
						sdP1 = pair.Component;
					}
					else
					{
						sdE2 = pair.Entity;
						sdP2 = pair.Component;
						break;
					}
				}

				// Safety check
				if (sdE1 == EntityRef.None || sdE2 == EntityRef.None)
					return;

				// First hit wins: healths are no longer equal
				if (sdP1->PlayerHealth != sdP2->PlayerHealth)
				{
					SuddenDeathResolved = true;
					
					if (sdP1->PlayerHealth < sdP2->PlayerHealth)
					{
						// P2 wins
						sdP1->IsDead = true;
						sdP2->ShowCelebration = true;
						sdP2->RoundsWon += 1;
						frame.Events.Dead(sdE1);
					}
					else
					{
						// P1 wins
						sdP2->IsDead = true;
						sdP1->ShowCelebration = true;
						sdP1->RoundsWon += 1;
						frame.Events.Dead(sdE2);
					}

					EndRound(frame, ERoundEndReason.KO);
				}

				// Still equal → wait for a hit
				return;
			}

			// ---------- TIMEOUT RESOLUTION ----------
			if (InGameMatchTime >= frame.RuntimeConfig.gameTime
			    && !frame.RuntimeConfig.training)
			{
				PlayerStat* p1 = null;
				PlayerStat* p2 = null;

				var e1 = EntityRef.None;
				var e2 = EntityRef.None;

				foreach (var pair in frame.Unsafe.GetComponentBlockIterator<PlayerStat>())
				{
					if (e1 == EntityRef.None)
					{
						e1 = pair.Entity;
						p1 = pair.Component;
					}
					else
					{
						e2 = pair.Entity;
						p2 = pair.Component;
						break;
					}
				}
				
				if (e1 == EntityRef.None || e2 == EntityRef.None)
					return;
				
				// SAME HEALTH → SUDDEN DEATH
				if (p1->PlayerHealth == p2->PlayerHealth)
				{
					SuddenDeath = true;
					InGameMatchTime = 0;
					
					return;
				}
				
				//NORMAL TIMEOUT
				EndRound(frame, ERoundEndReason.TimeOut);
				return;
			}

			// ---------- KO RESOLUTION ----------
			foreach (var pair in frame.Unsafe.GetComponentBlockIterator<PlayerStat>())
			{
				if (pair.Component->IsDead)
				{
					EndRound(frame, ERoundEndReason.KO);
					return;
				}
			}
		}
		
		private void PrepareReset(Frame frame)
		{
			RoundState = ERoundState.Reset;
			RoundTimer = 0;
			ResettingRound = true;
			SuddenDeath = false;
			frame.Events.PrepareRound();
		}
		
		private void EndRound(Frame frame, ERoundEndReason reason)
		{
			RoundState = ERoundState.End;
			RoundTimer = 0;

			CanFight = false;
			ResettingRound = true;

			frame.Events.RoundOver();
			
			//PLAYERS REGISTER DEATH
			foreach (var pair in frame.Unsafe.GetComponentBlockIterator<PlayerStat>())
			{
				switch (reason)
				{
					case ERoundEndReason.KO:
						pair.Component->CheckRoundEndKo(frame, pair.Entity);
						break;
					case ERoundEndReason.TimeOut:
						pair.Component->CheckRoundEndTimeOut(frame, pair.Entity);
						break;
				}
			}
		}
		
		private void AdvanceRound(Frame frame)
		{
			CurrentRound++;

			InGameMatchTime = 0;
			ResettingRound = false;
			
			if (CurrentRound > 3)
			{
				SetState(frame, EGameplayState.Finished);
				return;
			}

			ResetPlayers(frame);

			RoundState = ERoundState.Intro;
			RoundTimer = 0;
		}
		
		private void ResetPlayers(Frame frame)
		{
			var startPositions = frame.ResolveList(StartPositions);
			
			foreach (var pair in frame.Unsafe.GetComponentBlockIterator<PlayerStat>())
			{
				var playerAttack = frame.Unsafe.GetPointer<PlayerAttack>(pair.Entity);
				var playerMovement = frame.Unsafe.GetPointer<PlayerMovement>(pair.Entity);
				var config = frame.FindAsset<PlayerConfig>(playerAttack->playerConfig.Id);
				
				//Reset health
				pair.Component->PlayerHealth = config.maxHealth;
				pair.Component->IsDead = false;
				
				//Reset position
				var startPosition = frame.Unsafe.GetPointer<Transform3D>(startPositions[pair.Component->PlayerNumber]);
				playerMovement->Teleport(frame, pair.Entity, startPosition);
			}
		}

		private void CheckMatchOver(Frame frame)
		{
			foreach (var pair in frame.Unsafe.GetComponentBlockIterator<PlayerStat>())
			{
				//A player won
				if (pair.Component->RoundsWon >= 2)
				{
					WinnerEntity = pair.Entity;
					pair.Component->Won = true;
					SetState(frame, EGameplayState.Finished);
				}
			}
		}

		#endregion

		#region HANDLECONNECTIONS
		//Check player that disconnects 
		//Handle technical victory
		//Handle technical defeat
		//Ends game on disconnection
		private void CheckConnections(Frame frame)
		{
			if (DisconnectedEntity == EntityRef.None)
				return;

			if (DisconnectionResolved)
				return;

			if (State == EGameplayState.Finished)
				return;

			DisconnectionResolved = true;
			
			var loser = DisconnectedEntity;
			var loserMovement = frame.Unsafe.GetPointer<PlayerMovement>(loser);
			
			if (loserMovement->ClosestTarget == EntityRef.None)
				return;
			
			var winner = loserMovement->ClosestTarget;

			var winnerStat = frame.Unsafe.GetPointer<PlayerStat>(winner);
			var loserStat = frame.Unsafe.GetPointer<PlayerStat>(loser);
			
			winnerStat->ShowCelebration = true;
			winnerStat->Won = true;
			winnerStat->TechnicalVictory = true;
			
			loserStat->TechnicalDefeat = true;
			
			SetState(frame, EGameplayState.Finished);
		}
		
		#endregion

		#region CAMERAWORK

		private void CheckCloseCombat(Frame frame)
		{
		    var p1 = EntityRef.None;
		    var p2 = EntityRef.None;

		    Transform3D* t1 = null;
		    Transform3D* t2 = null;

		    PlayerAttack* a1 = null;
		    PlayerAttack* a2 = null;

		    // Collect the two players
		    foreach (var pair in frame.Unsafe.GetComponentBlockIterator<PlayerAttack>())
		    {
		        if (p1 == EntityRef.None)
		        {
		            p1 = pair.Entity;
		            a1 = pair.Component;
		            t1 = frame.Unsafe.GetPointer<Transform3D>(pair.Entity);
		        }
		        else
		        {
		            p2 = pair.Entity;
		            a2 = pair.Component;
		            t2 = frame.Unsafe.GetPointer<Transform3D>(pair.Entity);
		            break;
		        }
		    }

		    // Safety check
		    if (p1 == EntityRef.None || p2 == EntityRef.None)
		        return;

		    // Distance check
		    var distance = FPVector3.Distance(t1->Position, t2->Position);

		    // Close range condition
		    if (distance < FP.FromFloat_UNSAFE(2f) &&
		        (a1->isAttacking || a2->isAttacking))
		    {
			    InCloseCombat = true;
		    }
		    else
		    {
			    InCloseCombat = false;
		    }
		}

		#endregion

		#region GameFX
		private void CheckSlowMo(Frame frame)
		{
			var p1 = EntityRef.None;
			var p2 = EntityRef.None;
			
			PlayerMovement* m1 = null;
			PlayerMovement* m2 = null;

			// Collect the two players
			foreach (var pair in frame.Unsafe.GetComponentBlockIterator<PlayerMovement>())
			{
				if (p1 == EntityRef.None)
				{
					p1 = pair.Entity;
					m1 = pair.Component;
				}
				else
				{
					p2 = pair.Entity;
					m2 = pair.Component;
					break;
				}
			}

			// Safety check
			if (p1 == EntityRef.None || p2 == EntityRef.None)
				return;
			
			// Check player avoid hit
			if (m1->CheckSuccessfullyAvoidedHit() || m2->CheckSuccessfullyAvoidedHit())
			{
				AvoidSlowMo = true;
			}
		}

		private void HandleSlowMo(Frame frame)
		{
			var config = frame.FindAsset<GameConfig>(gameConfig.Id);
			
			if (AvoidSlowMo)
			{
				AvoidSlowMoTimer += frame.DeltaTime;

				if (AvoidSlowMoTimer >= config.avoidSlowMoTime)
				{
					AvoidSlowMo = false;
					AvoidSlowMoTimer = 0;
				}
			}
		}
		
		#endregion
		
		public QString64 AINameGenerator(Frame frame)
		{
			var prefix = prefixes[frame.RNG->Next(0, prefixes.Length)];
			var word = words[frame.RNG->Next(0, words.Length)];
			var suffix = suffixes[frame.RNG->Next(0, suffixes.Length)];

			return prefix + " " + word + (frame.RNG->Next(0, 2) == 0 ? "" : " " + suffix);
		}
	}
}











