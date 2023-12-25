using Microsoft.AspNetCore.Http;
using Azure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using System.ComponentModel;
using Task_Manager_Application.DTO;
using Task_Manager_Application.Entity;
using Container = Microsoft.Azure.Cosmos.Container;
using Task = Task_Manager_Application.Entity.Task;


namespace Task_Manager_Application.Controllers
{   
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        public string URI = "https://localhost:8081";
        public string PrimaryKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
        public string DatabaseName = "TaskDB";
        public string ContainerName = "TasksContainer";
        public readonly Container taskContainer;

    
        public TaskController() {
            taskContainer = GetContainer();
        }
        private Container GetContainer()
        {
            CosmosClient cosmosClient = new CosmosClient(URI,PrimaryKey);
            Database db = cosmosClient.GetDatabase(DatabaseName);
            Container container = db.GetContainer(ContainerName);
            return container;
        }

        

        [HttpPost]
        public async Task<IActionResult> AddTask(TaskDTO tDTO)
        {
            try
            {
                Task task = new Task();
                task.TaskId = tDTO.TaskId;
                task.TaskName = tDTO.TaskName;
                task.TaskDesc = tDTO.TaskDesc;
                
                task.Id = Guid.NewGuid().ToString();
                task.UId = task.Id;
                task.DocumentType = "task";

                task.CreatedOn = DateTime.Now;
                task.CreatedByName = "Sujal";
                task.CreatedBy = "Sujal's UID";

                task.UpdatedOn = DateTime.Now;
                task.UpdatedByName = "Sujal";
                task.UpdatedBy = "Sujal's UID";

                task.Version = 1;
                task.Active = true;
                task.Archieved = false;

                Task response = await taskContainer.CreateItemAsync(task); 
                
                tDTO.TaskName = response.TaskName;
                tDTO.TaskDesc = response.TaskDesc;

                return Ok(tDTO);
            }
            catch(Exception e)
            {
                return BadRequest("Data Adding failed,"+e.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> getAllTasks()
        {
            try
            {
                var Query = "select * from TasksContainer";
                List<Task> MyTasks = new List<Task>();

                using (FeedIterator<Task> entry = taskContainer.GetItemQueryIterator<Task>(Query))
                {

                    while (entry.HasMoreResults)
                    {
                        FeedResponse<Task> response = await entry.ReadNextAsync();
                        MyTasks.AddRange(response);
                    }
                }
                return Ok(MyTasks);
            }
            catch (Exception e)
            {
                return BadRequest("Failed to Retrive Data " + e.Message);
            }
        }

        [HttpGet]
        public IActionResult GetTaskById(String TaskId)
        {
            try
            {
                Task task = taskContainer.GetItemLinqQueryable<Task>(true).Where(q => q.TaskId == TaskId).AsEnumerable().FirstOrDefault();
                var taskModel = new TaskDTO();
                taskModel.TaskId = task.TaskId;
                taskModel.TaskName = task.TaskName;
                taskModel.TaskDesc = task.TaskDesc;
                return Ok(taskModel);
            }
            catch (Exception e)
            {
                return BadRequest("Failed to Retrive Data" + e.Message);
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteTask(string id, string taskName)
        {
            try
            {
                var response = await taskContainer.DeleteItemAsync<Task>(id, new PartitionKey(taskName));
                return Ok(response);
            }
            catch(Exception e)
            {
                return BadRequest("Failed to delete data "+ e.Message);
            }
        }

        [HttpPut]
        public async Task<Task> UpdateTask(string id,  TaskDTO task)
        {
            var item = taskContainer.GetItemLinqQueryable<Task>(true).Where(p => p.Id == id).AsEnumerable().FirstOrDefault();

            Task maintTask = new Task();
            maintTask.TaskId = task.TaskId;
            maintTask.TaskName = task.TaskName;
            maintTask.TaskDesc = task.TaskDesc;

            maintTask.Id = id;
            maintTask.UId = item.UId;
            maintTask.DocumentType = item.DocumentType;
            maintTask.CreatedBy = item.CreatedBy;
            maintTask.CreatedByName = item.CreatedByName;
            maintTask.CreatedOn = item.CreatedOn;
            maintTask.UpdatedBy = "New Person UID";
            maintTask.UpdatedByName = "New Person";
            maintTask.UpdatedOn = DateTime.Now;
            maintTask.Version = 1;
            maintTask.Active = true;
            maintTask.Archieved = false;

            var result = await taskContainer.ReplaceItemAsync(maintTask, id);

            return result;
        }  
    }
}
