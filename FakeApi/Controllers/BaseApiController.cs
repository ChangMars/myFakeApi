using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace FakeApi.Controllers
{
	[Route("api/baseapi")]
	//[Controller]
	//public class BaseApiController
	public class BaseApi : Controller
	{
		// GET: api/<TestApiController>
		[HttpGet]
		public IEnumerable<string> Get()
		{
			return new string[] { "value1", "value2" };
		}
	}
}
