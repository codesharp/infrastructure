using System;
using log4net;

namespace Castle.Services.Transaction
{
	public class DefaultTransactionManager : MarshalByRefObject, ITransactionManager
	{
		private static readonly ILog _Logger = LogManager.GetLogger(typeof (DefaultTransactionManager));
	
		private IActivityManager _ActivityManager;

		public event EventHandler<TransactionEventArgs> TransactionCreated;
		public event EventHandler<TransactionEventArgs> TransactionRolledBack;
		public event EventHandler<TransactionEventArgs> TransactionCompleted;
		public event EventHandler<TransactionEventArgs> ChildTransactionCreated;
		public event EventHandler<TransactionFailedEventArgs> TransactionFailed;
		public event EventHandler<TransactionEventArgs> TransactionDisposed;

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultTransactionManager"/> class.
		/// </summary>
		public DefaultTransactionManager() : this(new CallContextActivityManager())
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultTransactionManager"/> class.
		/// </summary>
		/// <exception cref="ArgumentNullException">activityManager is null</exception>
		/// <param name="activityManager">The activity manager.</param>
		public DefaultTransactionManager(IActivityManager activityManager)
		{
			if (activityManager == null) throw new ArgumentNullException("activityManager");

			_ActivityManager = activityManager;
			
			if (_Logger.IsDebugEnabled) _Logger.Debug("DefaultTransactionManager created.");
		}

		/// <summary>
		/// Gets or sets the activity manager.
		/// </summary>
		/// <exception cref="ArgumentNullException">value is null</exception>
		/// <value>The activity manager.</value>
		public IActivityManager ActivityManager
		{
			get { return _ActivityManager; }
			set
			{
				if (value == null) throw new ArgumentNullException("value");
				_ActivityManager = value;
			}
		}

		/// <summary>
		/// <see cref="ITransactionManager.CreateTransaction(Castle.Services.Transaction.TransactionMode,Castle.Services.Transaction.IsolationMode)"/>.
		/// </summary>
		public ITransaction CreateTransaction(TransactionMode txMode, IsolationMode isolationMode)
		{
			return CreateTransaction(txMode, isolationMode, false);
		}

		public ITransaction CreateTransaction(TransactionMode txMode, IsolationMode iMode, bool isAmbient)
		{
			txMode = ObtainDefaultTransactionMode(txMode);

			AssertModeSupported(txMode);

			if (CurrentTransaction == null &&
			    (txMode == TransactionMode.Supported ||
			     txMode == TransactionMode.NotSupported))
			{
				return null;
			}

			TransactionBase transaction = null;

			if (CurrentTransaction != null)
			{
				if (txMode == TransactionMode.Requires || txMode == TransactionMode.Supported)
				{
					transaction = ((TransactionBase)CurrentTransaction).CreateChildTransaction();

					_Logger.DebugFormat("Child transaction \"{0}\" created", transaction.Name);
				}
			}

			if (transaction == null)
			{
				transaction = InstantiateTransaction(txMode, iMode, isAmbient);

				if (isAmbient)
				{
#if MONO
					throw new NotSupportedException("Distributed transactions are not supported on Mono");
#else
					transaction.Enlist(new TransactionScopeResourceAdapter(txMode, iMode));
#endif
				}

				_Logger.DebugFormat("Transaction \"{0}\" created", transaction.Name);
			}

			_ActivityManager.CurrentActivity.Push(transaction);

			if (transaction.IsChildTransaction)
				ChildTransactionCreated.Fire(this, new TransactionEventArgs(transaction));
			else
				TransactionCreated.Fire(this, new TransactionEventArgs(transaction));

			return transaction;
		}

		private TransactionBase InstantiateTransaction(TransactionMode mode, IsolationMode isolationMode, bool ambient)
		{
			var t = new TalkativeTransaction(mode, isolationMode, ambient);

			t.TransactionCompleted += CompletedHandler;
			t.TransactionRolledBack += RolledBackHandler;
			t.TransactionFailed += FailedHandler;

			return t;
		}

		private void FailedHandler(object sender, TransactionFailedEventArgs e)
		{
			TransactionFailed.Fire(this, e);
		}

		private void RolledBackHandler(object sender, TransactionEventArgs e)
		{
			TransactionRolledBack.Fire(this, e);
		}

		private void CompletedHandler(object sender, TransactionEventArgs e)
		{
			TransactionCompleted.Fire(this, e);
		}

		private void AssertModeSupported(TransactionMode mode)
		{
			var ctx = CurrentTransaction;

			if (mode == TransactionMode.NotSupported &&
			    ctx != null &&
			    ctx.Status == TransactionStatus.Active)
			{
				var message = "There is a transaction active and the transaction mode " +
				              "explicit says that no transaction is supported for this context";

				_Logger.Error(message);

				throw new TransactionModeUnsupportedException(message);
			}
		}

		/// <summary>
		/// Gets the default transaction mode, i.e. the mode which is the current mode when
		/// <see cref="TransactionMode.Unspecified"/> is passed to <see cref="CreateTransaction(Castle.Services.Transaction.TransactionMode,Castle.Services.Transaction.IsolationMode)"/>.
		/// </summary>
		/// <param name="mode">The mode which was passed.</param>
		/// <returns>
		/// Requires &lt;- mode = Unspecified
		/// mode &lt;- otherwise
		/// </returns>
		protected virtual TransactionMode ObtainDefaultTransactionMode(TransactionMode mode)
		{
			return mode == TransactionMode.Unspecified ? TransactionMode.Requires : mode;
		}

		/// <summary>
		/// <see cref="ITransactionManager.CurrentTransaction"/>
		/// </summary>
		/// <remarks>Thread-safety of this method depends on that of the <see cref="IActivityManager.CurrentActivity"/>.</remarks>
		public ITransaction CurrentTransaction
		{
			get { return _ActivityManager.CurrentActivity.CurrentTransaction; }
		}

		/// <summary>
		/// <see cref="ITransactionManager.Dispose"/>.
		/// </summary>
		/// <param name="transaction"></param>
		public virtual void Dispose(ITransaction transaction)
		{
            //HACK:ȡ�������ͷ��쳣 changed by houkun 2010-9

            if (transaction == null)
            {
                _Logger.Debug("Tried to dispose a null transaction");
                return;
                //throw new ArgumentNullException("transaction", "Tried to dispose a null transaction");
            }
            if (CurrentTransaction != transaction)
            {
                _Logger.DebugFormat("Tried to dispose a transaction that is not on the current active transaction,{0}", transaction.Name);
                return;
                //throw new ArgumentException("Tried to dispose a transaction that is not on the current active transaction", "transaction");
            }

			_ActivityManager.CurrentActivity.Pop();

			if (transaction is IDisposable)
			{
				(transaction as IDisposable).Dispose();
			}

			if (transaction is IEventPublisher)
			{
				(transaction as IEventPublisher).TransactionCompleted -= CompletedHandler;
				(transaction as IEventPublisher).TransactionFailed -= FailedHandler;
				(transaction as IEventPublisher).TransactionRolledBack -= RolledBackHandler;
			}

			TransactionDisposed.Fire(this, new TransactionEventArgs(transaction));

			_Logger.DebugFormat("Transaction {0} disposed successfully", transaction.Name);
		}

		/// <summary>
		/// <see cref="MarshalByRefObject.InitializeLifetimeService"/>.
		/// </summary>
		/// <returns>always null</returns>
		public override object InitializeLifetimeService()
		{
			return null;
		}
	}
}