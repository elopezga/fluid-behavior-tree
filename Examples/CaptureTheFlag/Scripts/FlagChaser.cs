﻿using System.Collections;
using Adnc.FluidBT.Tasks;
using Adnc.FluidBT.Trees;
using UnityEngine;
using UnityEngine.AI;

namespace Adnc.FluidBT.Examples {
	public class FlagChaser : MonoBehaviour {
		public NavMeshAgent agent;
		public GameObject flagGraphic;
		public Team team;
		public bool defender;
		
		public enum Team {
			Red,
			Blue
		}

		private Vector3 _origin;
		private BehaviorTree _tree;
		private bool _stun;
		
		private GameObject Flag => FlagManager.current.flag;

		private GameObject Goal => team == Team.Blue ? 
			FlagManager.current.goalRed : FlagManager.current.goalBlue;

		private void Awake () {
			_origin = transform.position;
			
			_tree = new BehaviorTreeBuilder(gameObject)
				.Selector()
					.Condition(() => _stun)
					.Sequence("Capture Flag")
						.Condition("Not Carrying Flag", () => Flag != gameObject)
						.Selector()
							.Condition("Team Missing Flag", () => {
								var character = Flag.GetComponent<FlagChaser>();
								if (character == null) return true;
								return character.team != team;
							})
							.Condition("Not a Defender", () => !defender)
						.End()
						.Do(() => {
							agent.SetDestination(Flag.transform.position);

							if (Vector3.Distance(Flag.transform.position, transform.position) <= 1) {
								GrabFlag();
							} 

							return TaskStatus.Success;
						})
					.End()
					.Sequence("Score")
						.Condition("Has Flag", () => Flag == gameObject)
						.Do(() => {
							agent.SetDestination(Goal.transform.position);
					
							if (Vector3.Distance(Goal.transform.position, transform.position) <= 1) {
								ScoreGoal();
							} 
					
							return TaskStatus.Success;
						})
					.End()
					.Do("Return To Origin", () => {
						agent.SetDestination(_origin);
						return TaskStatus.Success;
					})
				.Build();
			
			flagGraphic.SetActive(false);
		}

		public void Stun () {
			StopCoroutine(StunLoop());
			StartCoroutine(StunLoop());
		}

		private IEnumerator StunLoop () {
			agent.ResetPath();
			_stun = true;
			yield return new WaitForSeconds(1);
			_stun = false;
		}

		private void ScoreGoal () {
			FlagManager.current.flagStart.SetActive(true);
			FlagManager.current.flag = FlagManager.current.flagStart;
		}

		private void GrabFlag () {
			Flag.SendMessage("Stun", SendMessageOptions.DontRequireReceiver);

			if (Flag == FlagManager.current.flagStart) {
				FlagManager.current.flagStart.SetActive(false);
			}
			
			FlagManager.current.flag = gameObject;
		}

		private void Update () {
			_tree.Tick();
			flagGraphic.SetActive(Flag == gameObject);
		}
	}
}
