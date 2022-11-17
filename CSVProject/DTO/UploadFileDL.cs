using CSVProject.Model;
using ExcelDataReader;
using MySql.Data.MySqlClient;
using System.Data.SqlClient;
using System.Data;
using System.Data.Common;

namespace CSVProject.DTO
{
    public class UploadFileDL : IUploadFileDL
    {
        public readonly IConfiguration _configuration;
        public readonly MySqlConnection _mySqlConnection;
        public UploadFileDL(IConfiguration configuration) 
        {
            _configuration = configuration;
            _mySqlConnection = new MySqlConnection(_configuration[key:"ConnectionStrings:EmployeeDataBase"]); 
        }
        public async Task<UploadFileResponse> UploadFile(UploadFileRequest request, string Path)
        {
            UploadFileResponse response = new UploadFileResponse();
            List<ExcelParameter> Parameters = new List<ExcelParameter>();

            response.IsSuccess = true;
            response.Message = "Successful";
            try
            {
                if (_mySqlConnection.State != System.Data.ConnectionState.Open)
                {
                    await _mySqlConnection.OpenAsync();
                }
                if (request.File.FileName.ToLower().Contains(value:".xlsx"))
                {
                    FileStream stream = new FileStream(Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                    IExcelDataReader reader = ExcelReaderFactory.CreateReader(stream);
                    DataSet dataSet = reader.AsDataSet(
                        configuration: new ExcelDataSetConfiguration()
                        { 
                            UseColumnDataType= false, 
                            ConfigureDataTable=(IExcelDataReader tableReader) => new ExcelDataTableConfiguration()
                            {
                                UseHeaderRow = true 
                            }
                        });
                    for(int i=0; i < dataSet.Tables[index:0].Rows.Count; i++)
                    {
                        ExcelParameter rows = new ExcelParameter();
                        rows.Name = dataSet.Tables[index: 0].Rows[i].ItemArray[0] != null ? Convert.ToString(dataSet.Tables[index: 0].Rows[i].ItemArray[0]) : "-1";
                        rows.City = dataSet.Tables[index: 0].Rows[i].ItemArray[1] != null ? Convert.ToString(dataSet.Tables[index: 0].Rows[i].ItemArray[1]) : "-1";
                        rows.Designation = dataSet.Tables[index: 0].Rows[i].ItemArray[2] != null ? Convert.ToString(dataSet.Tables[index: 0].Rows[i].ItemArray[2]) : "-1";
                        rows.Age = dataSet.Tables[index: 0].Rows[i].ItemArray[0] != null ? Convert.ToInt32(dataSet.Tables[index: 0].Rows[i].ItemArray[0]) : -1;
                        Parameters.Add(rows);
                    }
                    stream.Close();
                    if(Parameters.Count > 0)
                    {
                        string SqlQuery = @"INSERT INTO EmployeeDataBase.EmployeeData
                                            (Name, City, Designation, Age) VALUES
                                            (@Name, @City, @Designation, @Age)";
                        foreach(ExcelParameter rows in Parameters) 
                        {
                           using (MySqlCommand sqlCommand = new MySqlCommand(SqlQuery, _mySqlConnection)) 
                           {
                                sqlCommand.CommandType = CommandType.Text;
                                sqlCommand.CommandTimeout = 180;
                                sqlCommand.Parameters.AddWithValue(parameterName:"@Name",rows.Name);
                                sqlCommand.Parameters.AddWithValue(parameterName: "@City", rows.City);
                                sqlCommand.Parameters.AddWithValue(parameterName: "@Designation", rows.Designation);
                                sqlCommand.Parameters.AddWithValue(parameterName: "@Age", rows.Age);
                                int Status = await sqlCommand.ExecuteNonQueryAsync();
                                if (Status <= 0)
                                {
                                    response.IsSuccess = false;
                                    response.Message = "Query not executed";
                                    return response;
                                }
                            }
                        }
                    }
                }
                else 
                { 
                    response.IsSuccess = false;
                    response.Message = "Incorrect File";
                    return response;
                }
            }
            catch (Exception ex)
            { 
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            finally
            {
                await _mySqlConnection.CloseAsync();
                await _mySqlConnection.DisposeAsync();
            }
            return response;
        }
    }
}
