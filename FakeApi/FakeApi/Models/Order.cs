using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Stateless;

namespace FakeApi.Models
{
	//訂單有6種狀態
	public enum OrderStateEnum  // 名詞&形容詞(可持續)
	{
		Pending, // 訂單已生成
		Processing, // 支付處理中
		Completed, // 交易成功
		Declined, // 交易失敗
		Cancelled, // 訂單取消
		Refund, // 以退款
	}

	public enum OrderStateTriggerEnum // 動詞(事務的操作)
	{
		PlaceOrder, // 支付
		Approve, // 收款成功
		Reject, // 收款失败
		Cancel, // 取消
		Return // 退货
	}

	public class Order
	{
		public Order()
		{
			StateMachineInit();
		}

		[Key]
		public Guid Id { get; set; }

		public string UserId { get; set; }

		public ApplicationUser User { get; set; }

		public ICollection<LineItem> OrderItems { get; set; }

		public OrderStateEnum State { get; set; } // 使用有現狀態機

		public DateTime CreateDateUTC { get; set; }

		public string TransactionMetadata { get; set; } // 第三方支付

		StateMachine<OrderStateEnum, OrderStateTriggerEnum> _machine;

		public void PaymentProcessing()
		{
			_machine.Fire(OrderStateTriggerEnum.PlaceOrder);
		}

		public void PaymentApprove()
		{
			_machine.Fire(OrderStateTriggerEnum.Approve);
		}

		public void PaymentReject()
		{
			_machine.Fire(OrderStateTriggerEnum.Reject);
		}

		private void StateMachineInit() // 初始畫狀態機
		{
			_machine = new StateMachine<OrderStateEnum, OrderStateTriggerEnum>
				// (OrderStateEnum.Pending) // 直接設定初始狀態
				(() => State, s => State = s); // 轉換狀態

			_machine.Configure(OrderStateEnum.Pending) // 目前狀態為 Pending
				.Permit(OrderStateTriggerEnum.PlaceOrder, OrderStateEnum.Processing) // 如果動作為 支付 那狀態轉為 支付中
				.Permit(OrderStateTriggerEnum.Cancel, OrderStateEnum.Cancelled); // 如果動作為 取消 那狀態轉為 取消訂單

			_machine.Configure(OrderStateEnum.Processing) // 目前狀態為 Processing
				.Permit(OrderStateTriggerEnum.Approve, OrderStateEnum.Completed) // 如果動作為 收款成功 那狀態轉為 交易成功
				.Permit(OrderStateTriggerEnum.Reject, OrderStateEnum.Declined); // 如果動作為 收款失敗 那狀態轉為 交易失敗

			_machine.Configure(OrderStateEnum.Declined) // 目前狀態為 Declined
				.Permit(OrderStateTriggerEnum.PlaceOrder, OrderStateEnum.Processing); // 如果動作為 支付 那狀態轉為 支付中

			_machine.Configure(OrderStateEnum.Completed) // 目前狀態為 Completed
				.Permit(OrderStateTriggerEnum.Return, OrderStateEnum.Refund); // 如果動作為 退货 那狀態轉為 以退款
		}
	}
}
