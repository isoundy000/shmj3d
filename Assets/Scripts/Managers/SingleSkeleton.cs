using UnityEngine;
using System.Collections;

public class SingleSkeleton {
	static SingleSkeleton mInstance = null;

	bool inited = false;

	public static SingleSkeleton GetInstance () {
		if (mInstance == null)
			mInstance = new SingleSkeleton ();

		return mInstance;
	}

	public SingleSkeleton () {

	}

	public void Init() {
		if (inited)
			return;

		// TODO

		inited = true;
	}
}
